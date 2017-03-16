using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class AddNode : Node
    {
        public AddNode(Workspace scope) : base(scope)
        {
        }

        public Inlet<dynamic> A { get; set; }

        public Inlet<dynamic> B { get; set; }

        public Outlet<dynamic> Out { get; set; }

        protected override async Task Operate()
        {
            var a = await A;
            var b = await B;

            Out.Send(a + b);
        }
    }
}