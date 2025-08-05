using System;
using Echo.Interface;

namespace Echo.Core
{
      public readonly struct ScopedSubscription<T> : IEquatable<ScopedSubscription<T>>, IDisposable where T : struct, IEvent
      {
            private readonly Action<T> _action;
            private readonly bool _isFiltered;

            internal ScopedSubscription(Action<T> action, bool isFiltered = false)
            {
                  _action = action;
                  _isFiltered = isFiltered;
            }

            public void Dispose()
            {
                  if (_action != null)
                  {
                        if (_isFiltered)
                        {
                              EventBus.UnsubscribeFiltered(_action);
                        }
                        else
                        {
                              EventBus.Unsubscribe(_action);
                        }
                  }
            }

            public bool Equals(ScopedSubscription<T> other)
            {
                  return Equals(_action, other._action) && _isFiltered == other._isFiltered;
            }

            public override bool Equals(object obj)
            {
                  return obj is ScopedSubscription<T> other && Equals(other);
            }

            public override int GetHashCode()
            {
                  return HashCode.Combine(_action?.GetHashCode() ?? 0, _isFiltered);
            }
      }
}