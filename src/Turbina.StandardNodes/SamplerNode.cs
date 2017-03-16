using System;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public interface ISamplerNode : INode
    {
        IObserver<TimeSpan> Interval { get; }
        IObserver<dynamic> Value { get; }
        IObservable<dynamic> SampledValue { get; }
    }

    internal sealed class SamplerNode : Node, ISamplerNode
    {
        public Inlet<TimeSpan> Interval { get; set; }
        public Inlet<dynamic> Value { get; set; }
        public Outlet<dynamic> SampledValue { get; set; }

        IObserver<TimeSpan> ISamplerNode.Interval => Interval;
        IObserver<dynamic> ISamplerNode.Value => Value;
        IObservable<dynamic> ISamplerNode.SampledValue => SampledValue;

        public SamplerNode(Workspace workspace) : base(workspace)
        {
        }

        protected override async Task Operate()
        {
            while (true)
            {
                await Task.Delay(await Interval, NodeDisposedToken);
                SampledValue.Send(await Value);
            }
        }
    }
}