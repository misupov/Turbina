using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    [DebuggerDisplay("{" + nameof(GetDebugString) + "(),nq}")]
    public sealed class Inlet<T> : Pin, IObserver<T>
    {
        private readonly Inlet _inlet;

        public Inlet(Inlet inlet) : base(inlet.Node, inlet.Id)
        {
            _inlet = inlet;
        }

        public Inlet Proto => _inlet;

        public override IImmutableDictionary<string, string> Attributes => _inlet.Attributes;

        public override bool TryGetLatestValue(out object value)
        {
            return _inlet.TryGetLatestValue(out value);
        }

        public override Task<object> ObserveValue(CancellationToken cancellationToken)
        {
            return _inlet.ObserveValue(cancellationToken);
        }

        void IObserver<T>.OnCompleted()
        {
            // TODO
        }

        void IObserver<T>.OnError(Exception error)
        {
            // TODO
        }

        void IObserver<T>.OnNext(T value)
        {
            (_inlet as IObserver<object>).OnNext(value);
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return ReceiveOrTakeLatest().GetAwaiter();
        }

        public async Task<T> ReceiveOrTakeLatest()
        {
            return ValueConverter.Convert<object, T>(await _inlet.ReceiveOrTakeLatest());
        }

        private string GetDebugString()
        {
            if (_inlet.TryGetLatestValue(out object value))
            {
                return $"•{Id} = {value}";
            }
        
            return $"•{Id} = <not set>";
        }

        internal void Bind(Outlet outlet)
        {
            _inlet.Bind(outlet);
        }
    }
}