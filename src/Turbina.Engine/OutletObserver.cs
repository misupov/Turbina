using System;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    public sealed class OutletObserver<T>
    {
        private readonly Outlet<T> _outlet;
        private readonly TimeSpan _heartbeatInterval;
        private readonly Func<Task> _heartbeatTask;

        public OutletObserver(IObservable<T> outlet, TimeSpan heartbeatInterval, Func<Task> heartbeatTask)
        {
            if (outlet == null)
            {
                throw new ArgumentNullException(nameof(outlet));
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
            _outlet = (Outlet<T>)outlet;
        }

//        public async Task<T> ObserveValue()
//        {
//            var latestValueTask = _outlet.ObserveValue();
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