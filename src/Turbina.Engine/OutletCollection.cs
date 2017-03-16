using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Turbina.Engine
{
    public interface IOutletCollection
    {
        IReadOnlyList<Outlet> GetSnapshot();
    }

    public class OutletCollection : IOutletCollection
    {
        private readonly INode _node;
        private IImmutableList<Outlet> _outlets = ImmutableList<Outlet>.Empty;

        public OutletCollection(INode node)
        {
            _node = node;
        }

        internal void DisposeInternal()
        {
            foreach (var outlet in _outlets)
            {
                outlet.DisposeInternal();
            }
        }

        public IReadOnlyList<Outlet> GetSnapshot()
        {
            return _outlets;
        }

        public Outlet<T> Get<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _outlets.FirstOrDefault(outlet => outlet.Id == name).As<T>();
        }

        public Outlet GetOrCreate(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newOutlet = new Outlet(_node, name);

            var outlet = _outlets.FirstOrDefault(i => i.Id == name);
            while (outlet == null)
            {
                outlet = ImmutableInterlocked.Update(ref _outlets, list => list.Add(newOutlet))
                    ? newOutlet
                    : _outlets.FirstOrDefault(i => i.Id == name);
            }

            //            if (outlet == newOutlet)
            //            {
            //                OnCollectionChanged();
            //            }

            return outlet;
        }

        public Outlet<T> GetOrCreate<T>(string name)
        {
            return GetOrCreate(name).As<T>();
        }

        public Outlet Create(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var outlet = new Outlet(_node, name);

            while (!ImmutableInterlocked.Update(ref _outlets, list =>
            {
                if (list.Any(i => i.Id == name))
                {
                    throw new ArgumentException("An outlet with the same name already exists.", nameof(name));
                }
                return list.Add(outlet);
            }))
            {
            }

            //            OnCollectionChanged();

            return outlet;
        }

        public Outlet<T> Create<T>(string name)
        {
            return Create(name).As<T>();
        }
    }
}