using System;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      public readonly struct EventFilterBuilder<T> where T : struct, IEvent
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

                  var currentFilter = _filter;

                  return new EventFilterBuilder<T>(evt => currentFilter(evt) && condition(evt));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EventFilterBuilder<T> Or(Func<T, bool> condition)
            {
                  if (_filter == null)
                  {
                        return new EventFilterBuilder<T>(condition);
                  }

                  var currentFilter = _filter;

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
            public FilteredSubscription<T> SubscribeScoped(Action<T> handler)
            {
                  return _filter == null ? EventBus.SubscribeFilteredScoped(handler, static _ => true) : EventBus.SubscribeFilteredScoped(handler, _filter);
            }
      }
}