using System;
using System.Reflection;
using OpalStudio.Echo.Core.Diagnostics;
using Object = UnityEngine.Object;

namespace OpalStudio.Echo.Profiler.Editor.Model
{
      public sealed class SubscriberInfo
      {
            private readonly WeakReference<Delegate> _delegateRef;

            public Type EventType { get; }
            public SubscriptionKind Kind { get; }
            public DateTime SubscribedAt { get; }

            /// <summary>Cached method name (preserved even after the delegate dies).</summary>
            public string MethodName { get; }

            /// <summary>Cached declaring type name of the handler method.</summary>
            public string DeclaringTypeName { get; }

            /// <summary>Cached fully qualified declaring type, for IDE navigation.</summary>
            public Type DeclaringType { get; }

            /// <summary>The original target reference, if any (held weakly).</summary>
            private readonly WeakReference _targetRef;

            /// <summary>Number of times this handler has been observed firing.</summary>
            public int Hits { get; internal set; }

            public SubscriberInfo(Type eventType, Delegate handler, SubscriptionKind kind)
            {
                  EventType = eventType;
                  Kind = kind;
                  SubscribedAt = DateTime.UtcNow;
                  _delegateRef = new WeakReference<Delegate>(handler);

                  MethodInfo method = handler.Method;
                  MethodName = method.Name;
                  DeclaringType = method.DeclaringType;
                  DeclaringTypeName = method.DeclaringType?.Name ?? "<unknown>";

                  if (handler.Target != null)
                  {
                        _targetRef = new WeakReference(handler.Target);
                  }
            }

            public bool IsGhost
            {
                  get
                  {
                        if (!_delegateRef.TryGetTarget(out _))
                        {
                              return true;
                        }

                        if (_targetRef == null)
                        {
                              return false;
                        }

                        object target = _targetRef.Target;

                        if (target == null)
                        {
                              return true;
                        }

                        if (target is Object unityObj && unityObj == null)
                        {
                              return true;
                        }

                        return false;
                  }
            }

            public Object TryGetUnityTarget()
            {
                  if (_targetRef == null)
                  {
                        return null;
                  }

                  if (_targetRef.Target is Object unityObj && unityObj != null)
                  {
                        return unityObj;
                  }

                  return null;
            }

            internal bool TryGetDelegate(out Delegate handler)
            {
                  return _delegateRef.TryGetTarget(out handler);
            }
      }
}