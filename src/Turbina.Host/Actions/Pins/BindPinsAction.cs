using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Pins
{
    public class BindPinsAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var from = args["from"];
            var to = args["to"];

            var fromNodeId = (string)from["node"];
            var fromPinId = (string)from["pin"];

            var toNodeId = (string)to["node"];
            var toPinId = (string)to["pin"];

            var fromNode = workspace.GetNode(fromNodeId);
            var outlet = fromNode.Outlets.GetSnapshot().FirstOrDefault(o => o.Id == fromPinId);
            var toNode = workspace.GetNode(toNodeId);
            var inlet = toNode.Inlets.GetSnapshot().FirstOrDefault(o => o.Id == toPinId);

            workspace.Bind(outlet, inlet);

            await PostMessages(writer, workspace, messageId, CreateAddLinkMessage(fromNodeId, fromPinId, toNodeId, toPinId));
        }

        public static JObject CreateAddLinkMessage(string fromNodeId, string fromPinId, string toNodeId, string toPinId)
        {
            return new JObject
            {
                {"type", "add-link"},
                {"from-node", fromNodeId},
                {"from-pin", fromPinId},
                {"to-node", toNodeId},
                {"to-pin", toPinId}
            };
        }
    }
}