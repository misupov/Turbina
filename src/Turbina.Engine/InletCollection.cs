using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Turbina.Engine
{
    public interface IInletCollection
    {
        IReadOnlyList<Inlet> GetSnapshot();
    }

    public class InletCollection : IInletCollection
    {
        private readonly Node _node;
        private readonly WriteOnceBlock<bool> _buf;
        private readonly CancellationTokenSource _cts;
        private IImmutableList<Inlet> _inlets = ImmutableList<Inlet>.Empty;
        private ConcurrentDictionary<Inlet, Task> _inletsForAwait = new ConcurrentDictionary<Inlet, Task>();

        internal InletCollection(Node node)
        {
            _node = node;
            _cts = new CancellationTokenSource();
            _buf = new WriteOnceBlock<bool>(null, new DataflowBlockOptions
            {
                CancellationToken = _cts.Token
            });
        }

        internal void DisposeInternal()
        {
            foreach (var inlet in _inlets)
            {
                inlet.DisposeInternal();
            }
        }

        public IReadOnlyList<Inlet> GetSnapshot()
        {
            return _inlets;
        }

        public Inlet<T> Create<T>(string name, int bufferCapacity = 200)
        {
            return Create(name, bufferCapacity).As<T>();
        }

        public Inlet Create(string name, int bufferCapacity = 200)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var inlet = new Inlet(_node, name, bufferCapacity);

            while (!ImmutableInterlocked.Update(ref _inlets, list =>
            {
                if (list.Any(i => i.Id == name))
                {
                    throw new ArgumentException("An inlet with the same id already exists.", nameof(name));
                }
                return list.Add(inlet);
            }))
            {
            }

            RegisterInlet(inlet);

            return inlet;
        }

        public Inlet<T> Get<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _inlets.FirstOrDefault(outlet => outlet.Id == name).As<T>();
        }

        public Inlet GetOrCreate(string name, int bufferCapacity = 200)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newInlet = new Inlet(_node, name, bufferCapacity);

            var inlet = _inlets.FirstOrDefault(i => i.Id == name);
            while (inlet == null)
            {
                inlet = ImmutableInterlocked.Update(ref _inlets, list => list.Add(newInlet))
                    ? newInlet
                    : _inlets.FirstOrDefault(i => i.Id == name);
            }

            if (newInlet == inlet)
            {
                RegisterInlet(inlet);
            }

            return inlet;
        }

        public Inlet<T> GetOrCreate<T>(string name, int bufferCapacity = 200)
        {
            return GetOrCreate(name, bufferCapacity).As<T>();
        }

        public void Destroy(Inlet inlet)
        {
            while (true)
            {
                if (!_inlets.Contains(inlet))
                {
                    break;
                }
                if (ImmutableInterlocked.Update(ref _inlets, list => list.Remove(inlet)))
                {
                    inlet.DisposeInternal();
                    break;
                }
            }
        }

        public async Task AnyMessageAvailableAsync()
        {
            using (var cts1 = new CancellationTokenSource())
            {
                var cts2 = cts1;
                await Task.WhenAny(_inlets.Select(inlet => inlet.MessageAvailableAsync(cts2.Token).IgnoreCancellation()));
                cts2.Cancel();
            }
        }

        private void RegisterInlet(Inlet inlet)
        {
            _inletsForAwait.TryAdd(inlet, null);
        }
    }
}