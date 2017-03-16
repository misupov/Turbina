using System;

namespace Turbina.Engine
{
    public interface INode : IDisposable
    {
        string Id { get; }
        IObservable<Exception> Exception { get; }
        IInletCollection Inlets { get; }
        IOutletCollection Outlets { get; }
        INodeMetadata Metadata { get; }
        void Start();
    }

    public interface INodeMetadata
    {
        object this[string key] { get; set; }
    }
}