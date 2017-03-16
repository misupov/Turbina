using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Nodes
{
    public class RemoveNodesAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {

        }
    }
}