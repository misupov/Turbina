using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Turbina.Engine
{
    internal class InletBuffer<T> : IDisposable
    {
        private readonly string _name;
        private int _capacity;
        private IDisposable _subscription = Disposable.Empty;
        private readonly BufferBlock<T> _buf;
        private readonly CancellationTokenSource _cts;

        public InletBuffer(int capacity, string name)
        {
            _name = name;
            _capacity = capacity;
            _cts = new CancellationTokenSource();
            _buf = new BufferBlock<T>(new DataflowBlockOptions
            {
                BoundedCapacity = capacity,
                CancellationToken = _cts.Token
            });
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
            {
                return await _buf.ReceiveAsync(cts.Token);
            }
        }

        public Task OutputAvailableAsync(CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
            {
                return _buf.OutputAvailableAsync(cts.Token);
            }
        }

        public bool TryTake(out T value)
        {
            return _buf.TryReceive(out value);
        }

        public bool Post(T value)
        {
            return _buf.Post(value);
        }

        public void SetSource(Outlet outlet)
        {
            _subscription.Dispose();
            if (outlet != null)
            {
                _subscription = outlet.Subscribe(new AnonymousObserver<object>(o => Post((T)o)));
            }
        }


        //        private object _lock = new object();
        //        private readonly string _name;
        //        private int _capacity;
        //        private IDisposable _subscription = Disposable.Empty;
        //        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        //        private TaskCompletionSource<T> _takeAsync;
        //        private TaskCompletionSource<T> _peekAsync;
        //
        //        public InletBuffer(int capacity, string name)
        //        {
        //            _name = name;
        //            _capacity = capacity;
        //        }
        //
        //        public void Dispose()
        //        {
        //            lock (_lock)
        //            {
        //                var takeAsync = _takeAsync;
        //                var peekAsync = _peekAsync;
        //                Task.Run(() =>
        //                {
        //                    takeAsync?.TrySetCanceled();
        //                    peekAsync?.TrySetCanceled();
        //                });
        //            }
        //        }
        //
        //        public Task<T> TakeAsync()
        //        {
        //            lock (_lock)
        //            {
        //                if (_queue.TryDequeue(out T result))
        //                {
        //                    return Task.FromResult(result);
        //                }
        //
        //                var newCompletionSource = new TaskCompletionSource<T>();
        //                var oldCompletionSource = Interlocked.CompareExchange(ref _takeAsync, newCompletionSource, null);
        //                return oldCompletionSource?.Task ?? newCompletionSource.Task;
        //            }
        //        }
        //
        //        public Task<T> PeekAsync()
        //        {
        //            lock (_lock)
        //            {
        //                if (_queue.TryPeek(out T result))
        //                {
        //                    return Task.FromResult(result);
        //                }
        //
        //                var newCompletionSource = new TaskCompletionSource<T>();
        //                var oldCompletionSource = Interlocked.CompareExchange(ref _peekAsync, newCompletionSource, null);
        //                return oldCompletionSource?.Task ?? newCompletionSource.Task;
        //            }
        //        }
        //
        //        public bool TryTake(out T value)
        //        {
        //            lock (_lock)
        //            {
        //                return _queue.TryDequeue(out value);
        //            }
        //        }
        //
        //        public bool Post(T value)
        //        {
        //            lock (_lock)
        //            {
        //                if (_queue.Count >= _capacity)
        //                {
        //                    return false;
        //                }
        //
        //                var takeAsync = Interlocked.Exchange(ref _takeAsync, null);
        //                if (takeAsync != null)
        //                {
        //                    Task.Run(() => takeAsync.SetResult(value));
        //                }
        //                else
        //                {
        //                    _queue.Enqueue(value);
        //                    if (_queue.TryPeek(out value))
        //                    {
        //                        var peekAsync = Interlocked.Exchange(ref _peekAsync, null);
        //                        if (peekAsync != null)
        //                        {
        //                            Task.Run(() => peekAsync.SetResult(value));
        //                        }
        //                    }
        //                }
        //
        //                return true;
        //            }
        //        }
        //
        //        public void SetSource(Outlet outlet)
        //        {
        //            _subscription.Dispose();
        //            if (outlet != null)
        //            {
        //                _subscription = outlet.Subscribe(new AnonymousObserver<object>(o => this.Post((T)o)));
        //            }
        //        }
    }
}