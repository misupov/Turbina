using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Pins
{
    public class PushAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var nodeId = (string)args["node"];
            var pinId = (string)args["pin"];
            var value = (object)args["value"];
            var node = workspace.GetNode(nodeId);
            var inlet = node.Inlets.GetSnapshot().FirstOrDefault(i => i.Id == pinId);
            var type = Type.GetType(inlet.Attributes["type"]);
            var val = ((JToken) value).ToObject(type);
            ((IObserver<object>) inlet).OnNext(val);
        }
    }
}