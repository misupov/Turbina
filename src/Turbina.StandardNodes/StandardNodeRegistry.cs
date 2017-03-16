using System.Collections;
using System.Collections.Generic;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class StandardNodeRegistry : INodeRegistry
    {
        public IEnumerator<NodeInfo> GetEnumerator()
        {
            yield return new NodeInfo("and", typeof(AndNode));
            yield return new NodeInfo("or", typeof(OrNode));
            yield return new NodeInfo("not", typeof(NotNode));
            yield return new NodeInfo("debug", typeof(DebugNode));
            yield return new NodeInfo("http-request", typeof(HttpRequestNode));
            yield return new NodeInfo("timer", typeof(TimerNode));
            yield return new NodeInfo("sampler", typeof(SamplerNode));
            yield return new NodeInfo("add", typeof(AddNode));
            yield return new NodeInfo("keyboard-hook", typeof(KeyboardHookNode));
            yield return new NodeInfo("counter", typeof(MessageCounterNode));
            yield return new NodeInfo("expression", typeof(ExpressionNode));
            yield return new NodeInfo("random", typeof(RandomNode));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}