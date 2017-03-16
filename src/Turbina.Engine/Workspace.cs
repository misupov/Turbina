using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Turbina.Engine
{
    public class Workspace
    {
        public string Name { get; }

        private IImmutableList<NodeTypeDeclaration> _nodeTypes = ImmutableList<NodeTypeDeclaration>.Empty;
        private IImmutableList<INode> _nodes = ImmutableList<INode>.Empty;

        public Workspace(string name)
        {
            Name = name;
        }

        public void AddNodeRegistry(INodeRegistry registry)
        {
            foreach (var node in registry)
            {
                _nodeTypes = _nodeTypes.Add(new NodeTypeDeclaration(node.Name, node.Type));
            }
        }

        public INode CreateNode(string nodeType)
        {
            var declaration = _nodeTypes.FirstOrDefault(d => d.TypeName == nodeType);
            if (declaration != null)
            {
                var node = (INode)Activator.CreateInstance(declaration.Type, this);
                node.Start();

                while (!ImmutableInterlocked.Update(ref _nodes, list => list.Add(node)))
                {
                }

                return node;
            }

            throw new ArgumentException();
        }

        public void Reset()
        {
            var nodes = Interlocked.Exchange(ref _nodes, ImmutableList<INode>.Empty);

            foreach (var nodeInfo in nodes)
            {
                nodeInfo.Dispose();
            }
        }

        public void Bind<T1, T2>(IObservable<T1> outlet, IObserver<T2> inlet)
        {
            ((Inlet<T2>)inlet).Proto.Bind(((Outlet<T1>)outlet).Proto);
        }

        public void Bind(Outlet outlet, Inlet inlet)
        {
            inlet.Bind(outlet);
        }

        public void Unbind(Inlet inlet)
        {
            inlet.Unbind();
        }

        internal void RemoveNode(Node node)
        {
            while (!ImmutableInterlocked.Update(ref _nodes, list => list.Remove(node)))
            {
                if (!_nodes.Contains(node))
                {
                    break;
                }
            }
        }

        public IReadOnlyList<INode> Nodes => _nodes;

        public INode GetNode(string id)
        {
            return _nodes.First(node => node.Id == id);
        }

        private class NodeTypeDeclaration
        {
            public NodeTypeDeclaration(string typeName, Type type)
            {
                TypeName = typeName;
                Type = type;
            }

            public string TypeName { get; }
            public Type Type { get; }
        }
    }
}