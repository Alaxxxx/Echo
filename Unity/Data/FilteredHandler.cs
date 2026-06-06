using System;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Unity.Data
{
      internal readonly struct FilteredHandler<T> where T : struct, IEvent
      {
            public readonly Action<T> Handler;
            public readonly Func<T, bool> Filter;
            public readonly int Id;
            public readonly bool IsAlive;

            public FilteredHandler(Action<T> handler, Func<T, bool> filter, int id)
            {
                  Handler = handler;
                  Filter = filter;
                  Id = id;
                  IsAlive = true;
            }

            private FilteredHandler(Action<T> handler, Func<T, bool> filter, int id, bool isAlive)
            {
                  Handler = handler;
                  Filter = filter;
                  Id = id;
                  IsAlive = isAlive;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ShouldInvoke(T eventData) => Filter?.Invoke(eventData) ?? true;

            public FilteredHandler<T> MarkDead() => new FilteredHandler<T>(Handler, Filter, Id, false);
      }
}