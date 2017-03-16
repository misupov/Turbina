using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Turbina.Engine;
using Turbina.StandardNodes;

namespace Turbina.Host.Actions.Nodes
{
    public class AddNodeAction : Action
    {
        private static readonly string[] BgImages =
        {
            "../assets/img/bg/0.jpg",
            "../assets/img/bg/1.jpg",
            "../assets/img/bg/2.jpg",
            "../assets/img/bg/3.jpg",
            "../assets/img/bg/4.jpg",
            "../assets/img/bg/5.png",
            "../assets/img/bg/6.jpg",
            "../assets/img/bg/7.jpg",
            "../assets/img/bg/8.jpg",
            "../assets/img/bg/9.jpg",
            "../assets/img/bg/10.png",
            "../assets/img/bg/11.jpg",
            "../assets/img/bg/12.jpg",
            "../assets/img/bg/13.jpg",
            "../assets/img/bg/14.gif",
        };

        private static readonly Random Rnd = new Random();

        private static string GetRandomBackground()
        {
            return BgImages[Rnd.Next(BgImages.Length)];
        }

        public override async Task Process(StreamWriter writer, Workspace workspace, string messageId, JToken args)
        {
            var nodeType = (string)args["node-type"];
            var left = (double)args["left"];
            var top = (double)args["top"];
            var node = workspace.CreateNode(nodeType);
            node.Metadata["left"] = left;
            node.Metadata["top"] = top;
            if (node is IDebugNode)
            {
                node.Metadata["icon"] = "../assets/img/icons/debug.svg";
            }
            else if (node is IHttpRequestNode)
            {
                node.Metadata["icon"] = "../assets/img/icons/math-expression.svg";
            }
            else
            {
                node.Metadata["icon"] = "../assets/img/icons/stopwatch.svg";
            }
            node.Metadata["background-image"] = GetRandomBackground();
            var title = node.GetType().Name;

            const string suffix = "Node";
            if (title.EndsWith(suffix))
            {
                title = title.Substring(0, title.Length - suffix.Length);
            }
            node.Metadata["title"] = title;

            var timerNode = node as ITimerNode;
            if (timerNode != null)
            {
                timerNode.Interval.OnNext(TimeSpan.FromTicks(1));
                timerNode.IsEnabled.OnNext(true);
            }
//
//            var samplerNode = node as ISamplerNode;
//            if (samplerNode != null)
//            {
//                samplerNode.Interval.OnNext(TimeSpan.FromSeconds(1));
//                //
//                //                int i = 0;
//                //                var inletObserver = new InletObserver<dynamic>(samplerNode.Value, TimeSpan.FromSeconds(1), () =>
//                //                {
//                //                    i++;
//                //                    if (i < 20)
//                //                    {
//                //                        return Task.Delay(TimeSpan.FromSeconds(0.5));
//                //                    }
//                //                    else
//                //                    {
//                //                        return Task.Delay(TimeSpan.FromSeconds(5));
//                //                    }
//                //                });
//                //                Task.Run(async () =>
//                //                {
//                //                    while (true)
//                //                    {
//                //                        var value = (string) await inletObserver.ObserveValue();
//                //                        Debug.WriteLine(value);
//                //                    }
//                //                });
//            }


            await PostMessages(writer, workspace, messageId, CreateAddNodeMessage(node));
        }

        public static JObject CreateAddNodeMessage(INode node)
        {
            var left = (double)node.Metadata["left"];
            var top = (double)node.Metadata["top"];
            var icon = (string)node.Metadata["icon"];
            var backgroundImage = (string)node.Metadata["background-image"];
            var title = (string)node.Metadata["title"];

            var addNodeMessage = new JObject
            {
                ["type"] = "add-node",
                ["id"] = node.Id,
                ["icon"] = icon,
                ["title"] = title,
                ["left"] = left,
                ["top"] = top,
                ["background-image"] = backgroundImage,
                ["inlets"] = new JArray(node.Inlets.GetSnapshot().Select(PinDescription)),
                ["outlets"] = new JArray(node.Outlets.GetSnapshot().Select(PinDescription))
            };

            return addNodeMessage;
        }

        private static JObject PinDescription(Pin pin)
        {
            string type;
            var valueType = Type.GetType(pin.Attributes["type"]);
            if (valueType == typeof(bool))
            {
                type = "turbina-switch";
            }
            else if (valueType == typeof(TimeSpan))
            {
                type = "turbina-timespan";
            }
            else if (valueType == typeof(Uri))
            {
                type = "turbina-uri";
            }
            else
            {
                type = "turbina-text";
            }

            return new JObject
            {
                ["id"] = pin.Id,
                ["type"] = type,
                ["attributes"] = JToken.FromObject(pin.Attributes),
                ["advanced"] = valueType == typeof(Exception)
            };
        }
    }
}