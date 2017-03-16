using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    [DebuggerDisplay("{" + nameof(GetDebugString) + "(),nq}")]
    public sealed class Inlet : Pin, IObserver<object>
    {
        private readonly InletBuffer<object> _inletBuffer;
        private readonly LatestValueHolder<object> _latestValue = new LatestValueHolder<object>();
        private ImmutableDictionary<string, string> _attributes = ImmutableDictionary<string, string>.Empty;

        internal Inlet(INode node, string id, int bufferCapacity) : base(node, id)
        {
            _inletBuffer = new InletBuffer<object>(bufferCapacity, id);
        }

        public override IImmutableDictionary<string, string> Attributes => _attributes;

        public void SetAttribute(string attribute, string value)
        {
            ImmutableInterlocked.AddOrUpdate(ref _attributes, attribute, value, (_, __) => value);
        }

        internal void DisposeInternal()
        {
            _inletBuffer.Dispose();
            _latestValue.Dispose();
        }

        public Inlet<T> As<T>()
        {
            return new Inlet<T>(this);
        }

        public async Task<object> Receive(CancellationToken cancellationToken = default(CancellationToken))
        {
            var o = await _inletBuffer.TakeAsync(cancellationToken);
            _latestValue.SetValue(o);
            return o;
        }

        void IObserver<object>.OnCompleted()
        {
            // TODO
        }

        void IObserver<object>.OnError(Exception error)
        {
            // TODO
        }

        void IObserver<object>.OnNext(object value)
        {
            _inletBuffer.Post(value);
        }

        public async Task<object> ReceiveOrTakeLatest(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_inletBuffer.TryTake(out object o))
            {
                _latestValue.SetValue(o);
                return o;
            }

            if (_latestValue.TryGetValue(out var v))
            {
                return v;
            }

            return await Receive(cancellationToken);
        }

        public TaskAwaiter<object> GetAwaiter()
        {
            return ReceiveOrTakeLatest(CancellationToken.None).GetAwaiter();
        }

        internal async Task MessageAvailableAsync(CancellationToken cancellationToken)
        {
            await _inletBuffer.OutputAvailableAsync(cancellationToken);
        }

        internal void Bind(Outlet outlet)
        {
            BoundedOutlet = outlet;
            _latestValue.Reset();
            _inletBuffer.SetSource(outlet);
        }

        public void Unbind()
        {
            BoundedOutlet = null;
            _latestValue.Reset();
            _inletBuffer.SetSource(null);
        }

        private string GetDebugString()
        {
            if (_latestValue.TryGetValue(out object value))
            {
                return $"•{Id} = {value}";
            }

            return $"•{Id} = <not set>";
        }

        public override bool TryGetLatestValue(out object value)
        {
            if (_latestValue.TryGetValue(out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        public Outlet BoundedOutlet { get; private set; }

        public override async Task<object> ObserveValue(CancellationToken cancellationToken)
        {
            return await _latestValue.GetValue(cancellationToken);
        }
    }
}