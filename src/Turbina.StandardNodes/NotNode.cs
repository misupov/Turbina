using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class NotNode : Node
    {
        public NotNode(Workspace scope) : base(scope)
        {
        }

        public Inlet<dynamic> In { get; set; }

        public Outlet<dynamic> Out { get; set; }

        protected override async Task Operate()
        {
            var x = await In;

            var xIsTrue = x is bool && x == true || !(x is bool) && x > 0;
            Out.Send(!xIsTrue);
        }
    }
}