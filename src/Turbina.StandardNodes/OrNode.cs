using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class OrNode : Node
    {
        public OrNode(Workspace scope) : base(scope)
        {
        }

        public Inlet<dynamic> A { get; set; }

        public Inlet<dynamic> B { get; set; }

        public Outlet<dynamic> Out { get; set; }

        protected override async Task Operate()
        {
            var a = await A;
            var b = await B;

            var aIsTrue = a is bool && a == true || !(a is bool) && a > 0;
            var bIsTrue = b is bool && b == true || !(b is bool) && b > 0;
            Out.Send(aIsTrue || bIsTrue);
        }
    }
}