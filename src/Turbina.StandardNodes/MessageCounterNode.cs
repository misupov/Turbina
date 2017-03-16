using System;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class MessageCounterNode : Node
    {
        private int _totalCounter;
        private int _perSec;
        private int _lastTick;

        public Inlet<dynamic> Message { get; set; }

        public Outlet<int> Counter { get; set; }

        public Outlet<double> PerSec { get; set; }

        public MessageCounterNode(Workspace workspace) : base(workspace)
        {
        }

        protected override async Task Operate()
        {
            const int interval = 10000;
            var tickCount = Environment.TickCount;
            if (tickCount - _lastTick > interval)
            {
                PerSec.Send(_perSec * (1000.0 / interval));
                _lastTick = tickCount;
                _perSec = 0;
            }

            var message = await Message;
            Counter.Send(++_totalCounter);
            ++_perSec;
        }
    }
}