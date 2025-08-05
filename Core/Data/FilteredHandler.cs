using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core.Data
{
      internal readonly struct FilteredHandler<T> where T : struct, IEvent
      {
            public readonly Action<T> Handler;
            public readonly Func<T, bool> Filter;
            public readonly int Id;

            public FilteredHandler(Action<T> handler, Func<T, bool> filter, int id)
            {
                  Handler = handler;
                  Filter = filter;
                  Id = id;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ShouldInvoke(T eventData) => Filter?.Invoke(eventData) ?? true;
      }
}