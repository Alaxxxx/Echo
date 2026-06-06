using System;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      /// <summary>
      /// A disposable handle to an event subscription. Disposing it unsubscribes
      /// the associated handler from the event bus.
      /// </summary>
      /// <remarks>
      /// This struct is intentionally not <see cref="IEquatable{T}"/>. A subscription
      /// is a lifetime token, not a value, and equality between two tokens has no
      /// meaningful semantics.
      /// Calling <see cref="Dispose"/> more than once is safe.
      /// </remarks>
      public readonly struct ScopedSubscription<T> : IDisposable where T : struct, IEvent
      {
            private readonly Action<T> _action;

            internal ScopedSubscription(Action<T> action)
            {
                  _action = action;
            }

            public void Dispose()
            {
                  if (_action != null)
                  {
                        EventBus.Unsubscribe(_action);
                  }
            }
      }
}