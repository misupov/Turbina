using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    [DebuggerDisplay("{" + nameof(GetDebugString) + "(),nq}")]
    public sealed class Outlet : Pin, IObservable<object>
    {
        private readonly ReplaySubject<object> _subject = new ReplaySubject<object>(1);
        private readonly LatestValueHolder<object> _latestValue = new LatestValueHolder<object>();
        private ImmutableDictionary<string, string> _attributes = ImmutableDictionary<string, string>.Empty;

        public Outlet(INode node, string id) : base(node, id)
        {
        }

        public override IImmutableDictionary<string, string> Attributes => _attributes;

        public void SetAttribute(string attribute, string value)
        {
            ImmutableInterlocked.AddOrUpdate(ref _attributes, attribute, value, (_, __) => value);
        }

        public Outlet<T> As<T>()
        {
            return new Outlet<T>(this);
        }

        internal void DisposeInternal()
        {
            _subject.Dispose();
            _latestValue.Dispose();
        }

        public void Send(object value)
        {
            _latestValue.SetValue(value);
            _subject.OnNext(value);
        }

        public void SendError(Exception exception)
        {
            _subject.OnError(exception);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _subject.Subscribe(observer);
        }

        public override bool TryGetLatestValue(out object value)
        {
            if (_latestValue.TryGetValue(out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        private string GetDebugString()
        {
            if (_latestValue.TryGetValue(out var value))
            {
                return $"{Id}• = {value}";
            }

            return $"{Id}• = <not set>";
        }

        public override Task<object> ObserveValue(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}