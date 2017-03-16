using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Turbina.Engine
{
    internal class LatestValueHolder<T> : IDisposable
    {
        private volatile bool _hasValue;
        private T _value;
        private readonly BufferBlock<bool> _buf = new BufferBlock<bool>(new DataflowBlockOptions {BoundedCapacity = 1});
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public void Reset()
        {
            _hasValue = false;
            _buf.TryReceiveAll(out IList<bool> _);
        }

        public bool TryGetValue(out T value)
        {
            if (_hasValue)
            {
                value = _value;
                return true;
            }

            value = default(T);
            return false;
        }

        public void SetValue(T value)
        {
            _hasValue = true;
            _value = value;
            _buf.Post(true);
        }

        public async Task<T> GetValue(CancellationToken cancellationToken)
        {
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
            {
                await _buf.ReceiveAsync(linkedTokenSource.Token);
                return _value;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}