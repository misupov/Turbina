using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    public abstract class Pin
    {
        protected Pin(INode node, string id)
        {
            Node = node;
            Id = id;
        }

        public INode Node { get; }

        public string Id { get; }

        public abstract bool TryGetLatestValue(out object value);

        public abstract Task<object> ObserveValue(CancellationToken cancellationToken);

        public abstract IImmutableDictionary<string, string> Attributes { get; }
    }
}