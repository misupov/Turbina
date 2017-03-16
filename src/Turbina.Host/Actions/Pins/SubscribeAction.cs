using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Pins
{
    public class SubscribeAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var nodeId = (string)args["node"];
            var pinId = (string)args["pin"];
            var latestValue = (string)args["latestValue"];
            var node = workspace.GetNode(nodeId);
            var pin = (Pin) node.Inlets.GetSnapshot().FirstOrDefault(i => i.Id == pinId) ?? node.Outlets.GetSnapshot().FirstOrDefault(i => i.Id == pinId);
            if (pin != null)
            {
                if (pin.TryGetLatestValue(out var value) && latestValue != (string) GetJToken(value))
                {
                    await PostMessages(writer, workspace, messageId, CreateSnapshotMessage(nodeId, pinId, value));
                }
                else
                {
                    Task.Run(async () =>
                    {
                        value = await pin.ObserveValue(CancellationToken.None);
                        await PostMessages(writer, workspace, messageId, CreateSnapshotMessage(nodeId, pinId, value));
                    });
                }
            }
        }

        private static JObject CreateSnapshotMessage(string node, string pin, object value)
        {
            var token = GetJToken(value);

            return new JObject
            {
                {"type", "new-value"},
                {"node", node},
                {"pin", pin},
                {"value", token}
            };
        }

        private static JToken GetJToken(object value)
        {
            JToken token = null;

            if (value != null)
            {
                try
                {
                    if (value is Exception e)
                    {
                        return JToken.FromObject(e.Message);
                    }
                    token = JToken.FromObject(value);
                }
                catch
                {
                    token = JToken.FromObject(value.ToString());
                }
            }
            return token;
        }
    }
}