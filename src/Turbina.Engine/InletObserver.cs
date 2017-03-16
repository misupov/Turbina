using System;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
//    public sealed class InletObserver
//    {
//        private readonly Inlet _inlet;
//        private readonly TimeSpan _heartbeatInterval;
//        private readonly Func<Task> _heartbeatTask;
//
//        public InletObserver(Inlet inlet, TimeSpan heartbeatInterval, Func<Task> heartbeatTask)
//        {
//            if (inlet == null)
//            {
//                throw new ArgumentNullException(nameof(inlet));
//            }
//            if (heartbeatInterval.TotalMilliseconds <= 0)
//            {
//                throw new ArgumentException(nameof(heartbeatInterval));
//            }
//            if (heartbeatTask == null)
//            {
//                throw new ArgumentNullException(nameof(heartbeatTask));
//            }
//
//            _heartbeatInterval = heartbeatInterval;
//            _heartbeatTask = heartbeatTask;
//            _inlet = inlet;
//        }
//
//        public async Task<object> ObserveValue()
//        {
//            _inlet.TryGetLatestValue()
//            return _inlet.ObserveValue();
//        }
//    }

    public sealed class InletObserver<T>
    {
        private readonly Inlet<T> _inlet;
        private readonly TimeSpan _heartbeatInterval;
        private readonly Func<Task> _heartbeatTask;

        public InletObserver(IObserver<T> inlet, TimeSpan heartbeatInterval, Func<Task> heartbeatTask)
        {
            if (inlet == null)
            {
                throw new ArgumentNullException(nameof(inlet));
            }
            if (heartbeatInterval.TotalMilliseconds <= 0)
            {
                throw new ArgumentException(nameof(heartbeatInterval));
            }
            if (heartbeatTask == null)
            {
                throw new ArgumentNullException(nameof(heartbeatTask));
            }

            _heartbeatInterval = heartbeatInterval;
            _heartbeatTask = heartbeatTask;
            _inlet = (Inlet<T>)inlet;
        }

//        public async Task<T> ObserveValue()
//        {
//            var latestValueTask = _inlet.ObserveValue();
//            var heartbeatTask = _heartbeatTask();
//            var delayTaskCts = new CancellationTokenSource(_heartbeatInterval);
//            while (true)
//            {
//                var delayTask = Task.Delay(Timeout.Infinite, delayTaskCts.Token);
//
//                var resultTask = await Task.WhenAny(latestValueTask, heartbeatTask, delayTask);
//
//                if (resultTask == latestValueTask)
//                {
//                    delayTaskCts.Cancel();
//                    return latestValueTask.Result;
//                }
//
//                if (resultTask == heartbeatTask)
//                {
//                    delayTaskCts.Cancel();
//                    delayTaskCts = new CancellationTokenSource(_heartbeatInterval);
//                    heartbeatTask = _heartbeatTask();
//                }
//
//                if (resultTask == delayTask)
//                {
//                    delayTaskCts.Token.ThrowIfCancellationRequested();
//                }
//            }
//        }
    }
}