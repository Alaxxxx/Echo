using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core.Data
{
      public readonly struct EventFilterBuilder<T> : IEquatable<EventFilterBuilder<T>> where T : struct, IEvent
      {
            private readonly Func<T, bool> _filter;

            internal EventFilterBuilder(Func<T, bool> filter = null)
            {
                  _filter = filter;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EventFilterBuilder<T> And(Func<T, bool> condition)
            {
                  if (_filter == null)
                  {
                        return new EventFilterBuilder<T>(condition);
                  }

                  Func<T, bool> currentFilter = _filter;

                  return new EventFilterBuilder<T>(evt => currentFilter(evt) && condition(evt));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EventFilterBuilder<T> Or(Func<T, bool> condition)
            {
                  if (_filter == null)
                  {
                        return new EventFilterBuilder<T>(condition);
                  }

                  Func<T, bool> currentFilter = _filter;

                  return new EventFilterBuilder<T>(evt => currentFilter(evt) || condition(evt));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Subscribe(Action<T> handler)
            {
                  if (_filter == null)
                  {
                        EventBus.Subscribe(handler);
                  }
                  else
                  {
                        EventBus.SubscribeFiltered(handler, _filter);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ScopedSubscription<T> SubscribeScoped(Action<T> handler)
            {
                  Subscribe(handler);

                  return new ScopedSubscription<T>(handler);
            }

            public bool Equals(EventFilterBuilder<T> other)
            {
                  return Equals(_filter, other._filter);
            }

            public override bool Equals(object obj)
            {
                  return obj is EventFilterBuilder<T> other && Equals(other);
            }

            public override int GetHashCode()
            {
                  return (_filter != null ? _filter.GetHashCode() : 0);
            }
      }
}