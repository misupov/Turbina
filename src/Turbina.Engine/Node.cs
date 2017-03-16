using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    public abstract class Node : INode
    {
        private readonly CompositeDisposable _disposable;
        private int _operationsCount;
        private int _exceptionsCount;
        private bool _started;

        protected Node(Workspace workspace)
        {
            Id = Guid.NewGuid().ToString("N");
            var cts = new CancellationTokenSource();
            NodeDisposedToken = cts.Token;

            Inlets = new InletCollection(this);
            Outlets = new OutletCollection(this);

            _disposable = new CompositeDisposable
            {
                new CancellationDisposable(cts),
                Disposable.Create(() => Inlets.DisposeInternal()),
                Disposable.Create(() => Outlets.DisposeInternal()),
                Disposable.Create(() => workspace.RemoveNode(this))
            };

            SetupAttributtedPins();
        }

        public void Start()
        {
            if (!_started)
            {
                _started = true;
                OperateCore();
            }
        }

        protected virtual Task SetupNodes()
        {
            return Task.CompletedTask;
        }

        protected virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected virtual Task Destroy()
        {
            return Task.CompletedTask;
        }

        IObservable<Exception> INode.Exception => Exception;

        IInletCollection INode.Inlets => Inlets;

        IOutletCollection INode.Outlets => Outlets;

        public string Id { get; }

        public Outlet<Exception> Exception { get; set; }

        public CancellationToken NodeDisposedToken { get; }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public InletCollection Inlets { get; }

        public OutletCollection Outlets { get; }

        public INodeMetadata Metadata { get; } = new NodeMetadata();

        protected abstract Task Operate();

        private void OperateCore()
        {
            Task.Run(async () =>
            {
                await SetupNodes();
                await Initialize();
                await OperateLoop();
                await Destroy();
            }, NodeDisposedToken);
        }

        private async Task OperateLoop()
        {
            while (true)
            {
                await Inlets.AnyMessageAvailableAsync();
                try
                {
                    NodeDisposedToken.ThrowIfCancellationRequested();
                    var inletsBefore = Inlets.GetSnapshot();
                    await Operate();
                    _operationsCount++;
                    var inletsAfter = Inlets.GetSnapshot();
                    if (!Equals(inletsBefore, inletsAfter))
                    {
                    }
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken == NodeDisposedToken)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _exceptionsCount++;
                    Exception.Send(ex);
                }
            }
        }

        private void SetupAttributtedPins()
        {
            foreach (var propertyInfo in GetType().GetRuntimeProperties())
            {
                var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();
                if (propertyTypeInfo.IsGenericType && propertyTypeInfo.GetGenericTypeDefinition() == typeof(Inlet<>))
                {
                    var designerPropertiesAttributes = propertyInfo.GetCustomAttributes<DesignerPropertiesAttribute>();
                    var basicInlet = Inlets.Create(propertyInfo.Name);
                    basicInlet.SetAttribute("type", propertyTypeInfo.GenericTypeArguments[0].AssemblyQualifiedName);
                    var inlet = CastInlet(basicInlet, propertyTypeInfo.GenericTypeArguments[0]);
                    propertyInfo.SetValue(this, inlet);
                }
                if (propertyTypeInfo.IsGenericType && propertyTypeInfo.GetGenericTypeDefinition() == typeof(Outlet<>))
                {
                    var designerPropertiesAttributes = propertyInfo.GetCustomAttributes<DesignerPropertiesAttribute>();
                    var nonTypedOutlet = Outlets.Create(propertyInfo.Name);
                    nonTypedOutlet.SetAttribute("type", propertyTypeInfo.GenericTypeArguments[0].AssemblyQualifiedName);
                    var outlet = CastOutlet(nonTypedOutlet, propertyTypeInfo.GenericTypeArguments[0]);
                    propertyInfo.SetValue(this, outlet);
                }
            }
        }

        private object CastInlet(Inlet inlet, Type type)
        {
            return Activator.CreateInstance(typeof(Inlet<>).MakeGenericType(type), inlet);
        }

        private object CastOutlet(Outlet outlet, Type type)
        {
            return Activator.CreateInstance(typeof(Outlet<>).MakeGenericType(type), outlet);
        }

        private class NodeMetadata : INodeMetadata
        {
            private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();

            public object this[string key]
            {
                get { return _values[key]; }
                set { _values[key] = value; }
            }
        }
    }

    public class DesignerPropertiesAttribute : Attribute
    {
        
    }
}
