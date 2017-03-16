using System;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public interface IDebugNode : INode
    {
        IObserver<dynamic> Input { get; }
        IObservable<dynamic> Output { get; }
    }

    internal sealed class DebugNode : Node, IDebugNode
    {
        public Inlet<dynamic> In { get; set; }
        public Outlet<dynamic> Out { get; set; }

        public DebugNode(Workspace workspace) : base(workspace)
        {
        }

        IObserver<dynamic> IDebugNode.Input => In;

        IObservable<dynamic> IDebugNode.Output => Out;

        protected override async Task Operate()
        {
            var data = await In;
            Out.Send(data);
        }
    }
}