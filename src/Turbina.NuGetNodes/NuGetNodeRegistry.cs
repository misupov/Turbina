using System.Collections;
using System.Collections.Generic;
using Turbina.Engine;

namespace Turbina.NuGetNodes
{
    public class NuGetNodeRegistry : INodeRegistry
    {
        public IEnumerator<NodeInfo> GetEnumerator()
        {
            yield return new NodeInfo("nuget", typeof(NuGetListPackagesNode));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}