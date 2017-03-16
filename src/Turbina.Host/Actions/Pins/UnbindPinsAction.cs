using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Pins
{
    public class UnbindPinsAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var nodeId = (string)args["node"];
            var pinId = (string)args["pin"];

            var toNode = workspace.GetNode(nodeId);
            var inlet = toNode.Inlets.GetSnapshot().FirstOrDefault(o => o.Id == pinId);

            workspace.Unbind(inlet);

            await PostMessages(writer, workspace, messageId, CreateRemoveLinkMessage(nodeId, pinId));
        }

        public static JObject CreateRemoveLinkMessage(string nodeId, string pinId)
        {
            return new JObject
            {
                {"type", "remove-link"},
                {"node", nodeId},
                {"pin", pinId}
            };
        }
    }
}