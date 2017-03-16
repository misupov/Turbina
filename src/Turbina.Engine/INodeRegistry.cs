using System;
using System.Collections.Generic;

namespace Turbina.Engine
{
    public interface INodeRegistry : IEnumerable<NodeInfo>
    {
    }

    public class NodeInfo
    {
        public NodeInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }
}