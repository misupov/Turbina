using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turbina.Engine;

namespace Turbina.Host.Actions
{
    public abstract class Action
    {
        private static object _lock = new object();

        public abstract Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args);

        protected async Task PostMessages(StreamWriter writer, Workspace workspace, string messageId, params JObject[] messages)
        {
            lock (_lock)
            {
                var message = JsonConvert.SerializeObject(new { workspace = workspace.Name, corellationId = messageId, messages = messages });
                writer.WriteLine(message);
                writer.Flush();
            }
        }
    }
}