using System;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      internal static class Events<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            internal static void Invoke(T eventData)
            {
                  OnEvent?.Invoke(eventData);
            }

            internal static void Clear()
            {
                  OnEvent = null;
            }
      }
}