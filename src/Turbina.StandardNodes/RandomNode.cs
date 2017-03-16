using System;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class RandomNode : Node
    {
        private readonly Random _rnd = new Random();

        public RandomNode(Workspace scope) : base(scope)
        {
        }

        public Inlet<dynamic> Trigger { get; set; }

        public Outlet<double> Result { get; set; }

        protected override async Task Operate()
        {
            await Trigger;
            Result.Send(_rnd.NextDouble());
        }
    }
}