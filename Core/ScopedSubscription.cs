using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core
{
      /// <summary>
      /// Provides a scoped subscription to an event in the system. A scoped subscription is automatically
      /// unsubscribed when disposed or goes out of scope, ensuring safe and reliable event handling.
      /// </summary>
      /// <typeparam name="T">
      /// The type of event this subscription is associated with. Must be a struct implementing <see cref="IEvent"/>.
      /// </typeparam>
      /// <remarks>
      /// This struct uses an action to subscribe to the events, leveraging an internal event bus system.
      /// The lifecycle of the subscription is tightly coupled with the lifetime of the instance.
      /// </remarks>
      /// <threadsafety>
      /// This type is not guaranteed to be thread-safe. Proper synchronization must be applied if used in a multithreaded context.
      /// </threadsafety>
      /// <seealso cref="EventBus.SubscribeScoped{T}"/>
      /// <seealso cref="IEvent"/>
      public readonly struct ScopedSubscription<T> : IEquatable<ScopedSubscription<T>>, IDisposable where T : struct, IEvent
      {
            private readonly Action<T> _action;

            internal ScopedSubscription(Action<T> action)
            {
                  _action = action;
                  EventBus.Subscribe(action);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                  if (_action != null)
                  {
                        EventBus.Unsubscribe(_action);
                  }
            }

            public bool Equals(ScopedSubscription<T> other)
            {
                  return Equals(_action, other._action);
            }

            public override bool Equals(object obj)
            {
                  return obj is ScopedSubscription<T> other && Equals(other);
            }

            public override int GetHashCode()
            {
                  return (_action != null ? _action.GetHashCode() : 0);
            }
      }
}