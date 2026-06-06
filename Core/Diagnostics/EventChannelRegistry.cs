using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpalStudio.Echo.Core.Diagnostics
{
      /// <summary>
      /// Central registry of all event channels.
      ///
      /// Each <c>EventChannel&lt;T&gt;</c> registers itself here in its static
      /// constructor on first access, providing the bus with a way to
      /// list all known channels without reflection.
      ///
      /// The registry is safe to use from multiple threads during registration; channel-level operations
      /// (Subscribe, Publish, Unsubscribe) remain main-thread-only as documented
      /// on <see cref="EventBus"/>.
      /// </summary>
      public static class EventChannelRegistry
      {
            private static readonly ConcurrentDictionary<Type, Action> ClearActions = new();

            internal static void Register(Type eventType, Action clearAction)
            {
                  ClearActions.AddOrUpdate(eventType,
                        clearAction,
                        (_, existing) => existing + clearAction);
            }

            public static IReadOnlyCollection<Type> RegisteredEventTypes => (IReadOnlyCollection<Type>)ClearActions.Keys;

            public static void ClearType(Type eventType)
            {
                  if (ClearActions.TryGetValue(eventType, out Action clear))
                  {
                        clear();
                  }
            }

            public static void ClearAll()
            {
                  foreach (Action clear in ClearActions.Values)
                  {
                        clear();
                  }
            }
      }
}