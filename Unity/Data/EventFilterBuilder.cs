using System;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Unity.Data
{
      public readonly struct EventFilterBuilder<T> where T : struct, IEvent
      {
            private readonly Func<T, bool> _filter;

            private EventFilterBuilder(Func<T, bool> filter = null)
            {
                  _filter = filter;
            }

            public EventFilterBuilder<T> And(Func<T, bool> condition)
            {
                  if (_filter == null)
                  {
                        return new EventFilterBuilder<T>(condition);
                  }

                  Func<T, bool> currentFilter = _filter;

                  return new EventFilterBuilder<T>(evt => currentFilter(evt) && condition(evt));
            }

            public EventFilterBuilder<T> Or(Func<T, bool> condition)
            {
                  if (_filter == null)
                  {
                        return new EventFilterBuilder<T>(condition);
                  }

                  Func<T, bool> currentFilter = _filter;

                  return new EventFilterBuilder<T>(evt => currentFilter(evt) || condition(evt));
            }

            public void Subscribe(Action<T> handler)
            {
                  if (_filter == null)
                  {
                        Core.EventBus.Subscribe(handler);
                  }
                  else
                  {
                        EventBus.SubscribeFiltered(handler, _filter);
                  }
            }

            public FilteredSubscription<T> SubscribeScoped(Action<T> handler)
            {
                  return _filter == null ? EventBus.SubscribeFilteredScoped(handler, static _ => true) : EventBus.SubscribeFilteredScoped(handler, _filter);
            }
      }
}