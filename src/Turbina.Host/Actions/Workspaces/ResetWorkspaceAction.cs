using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions.Workspaces
{
    public class ResetWorkspaceAction : Action
    {
        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            workspace.Reset();

            await PostMessages(writer, workspace, messageId, CreateClearWorkspaceMessage());
        }

        public static JObject CreateClearWorkspaceMessage()
        {
            return new JObject
            {
                {"type", "reset-workspace"}
            };
        }
    }
}