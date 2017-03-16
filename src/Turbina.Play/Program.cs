using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turbina.Engine;
using Turbina.StandardNodes;

namespace Turbina.Play
{
    public class Program
    {
        private static Workspace workspace = new Workspace("123");

        public class Globals
        {
            public Dictionary<string, object> globals;
        }

        public static void Main(string[] args)
        {
            workspace.AddNodeRegistry(new StandardNodeRegistry());

            var timerNode = CreateTimer(TimeSpan.FromTicks(1));
            var debugNode = CreateDebug();

            workspace.Bind(timerNode.Counter, debugNode.Input);

            Console.WriteLine("1");

            Console.ReadKey();

            Task.Run(() => timerNode.IsEnabled.OnNext(false));

            Console.WriteLine("2");

            Console.ReadKey();

            Task.Run(() => timerNode.IsEnabled.OnNext(true));

            Console.WriteLine("3");

            Console.ReadKey();
            Console.WriteLine("EXIT");
        }

        private static ITimerNode CreateTimer(TimeSpan interval)
        {
            var node = (ITimerNode)workspace.CreateNode("timer");
            node.Interval.OnNext(interval);
            node.IsEnabled.OnNext(true);
            return node;
        }

        private static IDebugNode CreateDebug()
        {
            var node = (IDebugNode)workspace.CreateNode("debug");
            return node;
        }
    }
}