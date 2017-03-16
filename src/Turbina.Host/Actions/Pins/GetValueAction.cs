using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Pins
{
    public class GetValueAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var nodeId = (string)args["node"];
            var pinId = (string)args["pin"];
            var node = workspace.GetNode(nodeId);
            var inlet = node.Inlets.GetSnapshot().FirstOrDefault(i => i.Id == pinId);
            if (inlet != null)
            {
                inlet.TryGetLatestValue(out object value);
                await PostMessages(writer, workspace, messageId, CreateSnapshotMessage(nodeId, pinId, value));
            }
            else
            {
                var outlet = node.Outlets.GetSnapshot().FirstOrDefault(i => i.Id == pinId);
                if (outlet != null)
                {
                    outlet.TryGetLatestValue(out object value);
                    await PostMessages(writer, workspace, messageId, CreateSnapshotMessage(nodeId, pinId, value));
                }
            }
        }

        private static JObject CreateSnapshotMessage(string node, string pin, object value)
        {
            JToken token = null;

            if (value != null)
            {
                try
                {
                    token = JToken.FromObject(value);
                }
                catch
                {
                    token = JToken.FromObject(value.ToString());
                }
            }

            return new JObject
            {
                {"type", "value-snapshot"},
                {"node", node},
                {"pin", pin},
                {"value", token}
            };
        }
    }
}