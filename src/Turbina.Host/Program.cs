using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turbina.Engine;
using Turbina.Host.Actions.Nodes;
using Turbina.Host.Actions.Pins;
using Turbina.Host.Actions.Workspaces;
using Turbina.StandardNodes;
using Action = Turbina.Host.Actions.Action;

namespace Turbina.Host
{
    public class Program
    {
        private static readonly Dictionary<string, Dictionary<string, Action>> Actions = new Dictionary<string, Dictionary<string, Action>>
        {
            ["workspace"] = new Dictionary<string, Action>
            {
                ["reset"] = new ResetWorkspaceAction(),
                ["load"] = new LoadWorkspaceAction()
            },
            ["node"] = new Dictionary<string, Action>
            {
                ["add"] = new AddNodeAction(),
                ["remove"] = new RemoveNodesAction()
            },
            ["pin"] = new Dictionary<string, Action>
            {
                ["bind"] = new BindPinsAction(),
                ["unbind"] = new UnbindPinsAction(),
                ["get-value"] = new GetValueAction(),
                ["subscribe"] = new SubscribeAction(),
                ["push"] = new PushAction()
            }
        };

        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

            Task.Run(MainLoop);

            while (true)
            {
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
            }
        }

        public static ConcurrentDictionary<string, Workspace> Workspaces { get; } = new ConcurrentDictionary<string, Workspace>();

        private static async Task MainLoop()
        {
            var tcpListener = new TcpListener(IPAddress.Any, 57000);
            tcpListener.Start();
            while (true)
            {
                try
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    ProcessClient(client);
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        private static async void ProcessClient(TcpClient client)
        {
            using (var input = client.GetStream())
            using (var reader = new StreamReader(input))
            using (var writer = new StreamWriter(input))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    try
                    {
                        var message = (JObject)JsonConvert.DeserializeObject(line);
                        if (message == null)
                        {
                            continue;
                        }

                        var id = (string)message["id"];
                        var type = (string)message["type"];
                        var action = (string)message["action"];
                        var messageArgs = message["args"];
                        var workspaceName = (string)message["workspace"] ?? "<default-workspace>"; // todo: should be mandatory
                        var workspace = Workspaces.GetOrAdd(workspaceName, CreateWorkspace);

                        if (Actions.TryGetValue(type, out var actionList))
                        {
                            if (actionList.TryGetValue(action, out var action1))
                            {
                                await action1.Process(writer, workspace, id, messageArgs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        Console.Out.WriteLine(e.ToString());
                    }
                }
            }
        }

        private static Workspace CreateWorkspace(string workspaceName)
        {
            var workspace = new Workspace(workspaceName);
            workspace.AddNodeRegistry(new StandardNodeRegistry());
            return workspace;
        }
    }
}