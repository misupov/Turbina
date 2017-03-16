using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;
using Turbina.Host.Actions.Nodes;
using Turbina.Host.Actions.Pins;

namespace Turbina.Host.Actions.Workspaces
{
    public class LoadWorkspaceAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            await PostMessages(writer, workspace, messageId, ResetWorkspaceAction.CreateClearWorkspaceMessage());

            var addNodeMessages = workspace.Nodes.Select(AddNodeAction.CreateAddNodeMessage).ToArray();

            if (addNodeMessages.Any())
            {
                await PostMessages(writer, workspace, messageId, addNodeMessages.ToArray());
            }

            var pairs = workspace.Nodes
                .SelectMany(node => node.Inlets.GetSnapshot())
                .Select(inlet => new { inlet, outlet = inlet.BoundedOutlet })
                .Where(pair => pair.outlet != null);

            var addLinkMessages = pairs.Select(pair => BindPinsAction.CreateAddLinkMessage(pair.outlet.Node.Id,
                pair.outlet.Id,
                pair.inlet.Node.Id,
                pair.inlet.Id)).ToArray();

            if (addLinkMessages.Any())
            {
                await PostMessages(writer, workspace, messageId, addLinkMessages);
            }
        }
    }
}