using System;
using OpalStudio.Echo.Core.Diagnostics;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      /// <summary>
      /// Internal storage for the event handlers of a given event type.
      /// One distinct closed generic type is created per event type T.
      /// </summary>
      internal static class EventChannel<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            internal static Action<T> SecondaryDispatch;

            internal static Func<bool> SecondaryHasSubscribers;
            static EventChannel()
            {
                  EventChannelRegistry.Register(typeof(T), Clear);
            }

            internal static void Invoke(T eventData)
            {
                  Action<T> handler = OnEvent;

                  long startTicks = 0;
                  EchoInstrumentation.CaptureTimestamp(ref startTicks);
                  EchoInstrumentation.NotifyDispatchStart(in eventData);

                  var instrumented = false;
                  EchoInstrumentation.QueryHandlerInstrumentation(ref instrumented);

                  if (instrumented)
                  {
                        InvokeInstrumented(handler, eventData);
                  }
                  else
                  {
                        handler?.Invoke(eventData);
                  }

                  SecondaryDispatch?.Invoke(eventData);

                  long endTicks = 0;
                  EchoInstrumentation.CaptureTimestamp(ref endTicks);
                  EchoInstrumentation.NotifyDispatchEnd(typeof(T), endTicks - startTicks);
            }

            private static void InvokeInstrumented(Action<T> handler, T eventData)
            {
                  if (handler == null)
                  {
                        return;
                  }

                  Delegate[] list = handler.GetInvocationList();

                  for (var i = 0; i < list.Length; i++)
                  {
                        EchoInstrumentation.NotifyHandlerInvoked(typeof(T), list[i]);
                        ((Action<T>)list[i]).Invoke(eventData);
                  }
            }

            internal static bool HasSubscribers => OnEvent != null || (SecondaryHasSubscribers != null && SecondaryHasSubscribers());

            private static void Clear()
            {
                  OnEvent = null;
            }
      }
}