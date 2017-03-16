using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    [DebuggerDisplay("{" + nameof(GetDebugString) + "(),nq}")]
    public class Outlet<T> : Pin, IObservable<T>
    {
        private readonly Outlet _outlet;

        public Outlet(Outlet outlet) : base(outlet.Node, outlet.Id)
        {
            _outlet = outlet;
        }

        public Outlet Proto => _outlet;

        public override IImmutableDictionary<string, string> Attributes => _outlet.Attributes;

        public void Send(T value)
        {
            _outlet.Send(value);
        }

        public void SendError(Exception exception)
        {
            _outlet.SendError(exception);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _outlet.Subscribe(new AnonymousObserver<object>(o =>
                {
                    var val = ValueConverter.Convert<object, T>(o);
                    observer.OnNext(val);
                },
                observer.OnError,
                observer.OnCompleted));
        }

        private string GetDebugString()
        {
            if (_outlet.TryGetLatestValue(out object value))
            {
                return $"{Id}• = {value}";
            }

            return $"{Id}• = <not set>";
        }

        public override bool TryGetLatestValue(out object value)
        {
            return _outlet.TryGetLatestValue(out value);
        }

        public override Task<object> ObserveValue(CancellationToken cancellationToken)
        {
            return _outlet.ObserveValue(cancellationToken);
        }
    }
}