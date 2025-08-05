using System;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      public readonly struct ScopedSubscription<T> : IEquatable<ScopedSubscription<T>>, IDisposable where T : struct, IEvent
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
                  return _action?.GetHashCode() ?? 0;
            }
      }
}