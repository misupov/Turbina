using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public interface ITimerNode : INode
    {
        IObserver<bool> IsEnabled { get; }
        IObserver<TimeSpan> Interval { get; }
        IObserver<bool> UseLocalTimeZone { get; }
        IObservable<int> Counter { get; }
        IObservable<DateTimeOffset> Timestamp { get; }
    }

    internal sealed class TimerNode : Node, ITimerNode
    {
        public Inlet<bool> IsEnabled { get; set; }
        public Inlet<TimeSpan> Interval { get; set; }
        public Inlet<bool> UseLocalTimeZone { get; set; }

        public Outlet<int> Counter { get; set; }
        public Outlet<DateTimeOffset> Timestamp { get; set; }

        IObserver<bool> ITimerNode.IsEnabled => IsEnabled;
        IObserver<TimeSpan> ITimerNode.Interval => Interval;
        IObserver<bool> ITimerNode.UseLocalTimeZone => UseLocalTimeZone;
        IObservable<int> ITimerNode.Counter => Counter;
        IObservable<DateTimeOffset> ITimerNode.Timestamp => Timestamp;

        public TimerNode(Workspace workspace) : base(workspace)
        {
            (UseLocalTimeZone as IObserver<bool>).OnNext(true); // TODO
        }

        protected override async Task Operate()
        {
            var currentIsEnabled = false;
            var currentInterval = default(TimeSpan);
            var stopwatch = Stopwatch.StartNew();
            var counter = 0;
            var ticksElapsed = 0;

            while (true)
            {
                var newIsEnabled = await IsEnabled;
                var newInterval = await Interval;
                var useLocalTimeZone = await UseLocalTimeZone;

                var shouldPost = true;
                if (currentIsEnabled != newIsEnabled || currentInterval != newInterval)
                {
                    shouldPost = false;
                    stopwatch = Stopwatch.StartNew();
                    ticksElapsed = 0;
                    currentIsEnabled = newIsEnabled;
                    currentInterval = newInterval;
                    if (!currentIsEnabled || currentInterval.TotalMilliseconds <= 0)
                    {
                        break;
                    }
                }

                if (shouldPost)
                {
                    ticksElapsed++;
                    counter++;
                    Counter.Send(counter);
                    Timestamp.Send(useLocalTimeZone ? DateTimeOffset.Now : DateTimeOffset.UtcNow);
                }

                using (var cts1 = new CancellationTokenSource())
                using (var cts2 = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, NodeDisposedToken))
                {
                    var delayTask = GetDelayTask(stopwatch.Elapsed, ticksElapsed, currentInterval, cts2.Token);
                    if (delayTask.Status != TaskStatus.RanToCompletion)
                    {
                        await Task.WhenAny(
                            delayTask.IgnoreCancellation(),
                            Inlets.AnyMessageAvailableAsync());

                        cts1.Cancel();
                    }
                }

                NodeDisposedToken.ThrowIfCancellationRequested();
            }
        }

        private static Task GetDelayTask(TimeSpan totalElapsed, int ticksElapsed, TimeSpan interval, CancellationToken token)
        {
            var delta = ticksElapsed * interval.TotalMilliseconds - totalElapsed.TotalMilliseconds;
            return delta > 0
                ? Task.Delay(interval, token)
                : Task.CompletedTask;
        }
    }
}