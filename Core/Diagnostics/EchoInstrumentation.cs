using System;
using System.Diagnostics;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Diagnostics
{
      /// <summary>
      /// Instrumentation hooks for the Core event bus. Used by the
      /// <c>Echo.Profiler.Editor</c> assembly to observe dispatches without
      /// the Core itself depending on UnityEditor.
      /// </summary>
      /// <remarks>
      /// All hook-invocation methods are decorated with
      /// <see cref="ConditionalAttribute"/> so the compiler strips them entirely
      /// from release builds. In the editor (or when <c>ECHO_PROFILER</c> is
      /// defined), each invocation reduces to a single null-check on a static
      /// delegate field.
      /// </remarks>
      internal static class EchoInstrumentation
      {
            /// <summary>
            /// Invoked at the start of a Publish, before any handler runs.
            /// </summary>
            public static Action<Type, object> OnDispatchStart;

            /// <summary>
            /// Invoked at the end of a Publish, after all handlers have completed.
            /// </summary>
            public static Action<Type, long> OnDispatchEnd;

            /// <summary>
            /// Invoked whenever a handler is added to a channel.
            /// </summary>
            public static Action<Type, Delegate, SubscriptionKind> OnSubscribe;

            /// <summary>
            /// Invoked whenever a handler is removed from a channel.
            /// </summary>
            public static Action<Type, Delegate> OnUnsubscribe;

            /// <summary>
            /// Invoked just before each handler runs inside a dispatch.
            /// </summary>
            public static Action<Type, Delegate> OnHandlerInvoked;

            /// <summary>
            /// Reads the current high-resolution timestamp.
            /// </summary>
            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void CaptureTimestamp(ref long timestamp)
            {
                  timestamp = Stopwatch.GetTimestamp();
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void NotifyDispatchStart<T>(in T eventData) where T : struct, IEvent
            {
                  Action<Type, object> hook = OnDispatchStart;

                  if (hook == null)
                  {
                        return;
                  }

                  hook(typeof(T), eventData);
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void NotifyDispatchEnd(Type eventType, long elapsedTicks)
            {
                  OnDispatchEnd?.Invoke(eventType, elapsedTicks);
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void NotifySubscribe(Type eventType, Delegate handler, SubscriptionKind kind)
            {
                  OnSubscribe?.Invoke(eventType, handler, kind);
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void NotifyUnsubscribe(Type eventType, Delegate handler)
            {
                  OnUnsubscribe?.Invoke(eventType, handler);
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void NotifyHandlerInvoked(Type eventType, Delegate handler)
            {
                  OnHandlerInvoked?.Invoke(eventType, handler);
            }

            [Conditional("UNITY_EDITOR")]
            [Conditional("ECHO_PROFILER")]
            internal static void QueryHandlerInstrumentation(ref bool instrumented)
            {
                  instrumented = OnHandlerInvoked != null;
            }
      }

      /// <summary>
      /// Classification of how a handler was registered. Surfaced in the
      /// Subscribers panel of the monitor.
      /// </summary>
      public enum SubscriptionKind
      {
            /// <summary>A plain <c>EventBus.Subscribe</c> call.</summary>
            Plain,

            /// <summary>A scoped subscription returning <c>ScopedSubscription&lt;T&gt;</c>.</summary>
            Scoped,

            /// <summary>A filtered subscription (Unity layer only).</summary>
            Filtered,

            /// <summary>A one-shot subscription registered via <c>SubscribeOnce</c>.</summary>
            Once
      }
}