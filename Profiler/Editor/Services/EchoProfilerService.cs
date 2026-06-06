using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpalStudio.Echo.Core.Diagnostics;
using OpalStudio.Echo.Profiler.Editor.Model;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      [InitializeOnLoad]
      public static class EchoProfilerService
      {
            private static readonly ConcurrentQueue<IPendingChange> Pending = new();
            private static readonly Dictionary<Type, ChannelModel> Channels = new();
            private static readonly RingBuffer<DispatchRecord> GlobalFeed = new(GlobalFeedCapacity);

            private static readonly Dictionary<Type, ProfilerMarker> Markers = new();

            public const int GlobalFeedCapacity = 512;

            public static bool IsLive { get; private set; } = true;
            public static bool CapturePayloads
            {
                  get => EchoMonitorSettings.CapturePayloads;
                  set => EchoMonitorSettings.CapturePayloads = value;
            }

            public static IReadOnlyDictionary<Type, ChannelModel> AllChannels => Channels;
            public static RingBuffer<DispatchRecord> Feed => GlobalFeed;

            public static long TotalDispatches { get; private set; }

            public static int GhostCount { get; private set; }

            public static event Action<ChannelModel> ChannelDiscovered;
            public static event Action ModelChanged;

            public static event Action<DispatchRecord> DispatchObserved;

            private static readonly Stack<PendingDispatch> PendingDispatches = new();

            private readonly struct PendingDispatch
            {
                  public readonly Type EventType;
                  public readonly object Payload;
                  public readonly DateTime Timestamp;
                  public readonly ProfilerMarker.AutoScope Scope;

                  public PendingDispatch(Type eventType, object payload, DateTime timestamp, ProfilerMarker.AutoScope scope)
                  {
                        EventType = eventType;
                        Payload = payload;
                        Timestamp = timestamp;
                        Scope = scope;
                  }
            }

            static EchoProfilerService()
            {
                  Hook();
                  EditorApplication.update += FlushPending;
                  EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            public static void SetLive(bool live)
            {
                  if (IsLive == live)
                  {
                        return;
                  }

                  IsLive = live;
                  ModelChanged?.Invoke();
            }

            public static void Clear()
            {
                  Channels.Clear();
                  GlobalFeed.Clear();
                  TotalDispatches = 0;
                  GhostCount = 0;
                  ModelChanged?.Invoke();
            }

            public static int PruneGhosts()
            {
                  var removed = 0;

                  foreach (ChannelModel channel in Channels.Values)
                  {
                        for (int i = channel.Subscribers.Count - 1; i >= 0; i--)
                        {
                              SubscriberInfo info = channel.Subscribers[i];

                              if (info.IsGhost)
                              {
                                    Delegate handler = info.TryGetDelegate(out Delegate d) ? d : null;
                                    channel.RemoveSubscriber(handler);
                                    removed++;
                              }
                        }
                  }

                  if (removed > 0)
                  {
                        ModelChanged?.Invoke();
                  }

                  return removed;
            }

            public static int CountGhosts()
            {
                  var total = 0;

                  foreach (ChannelModel channel in Channels.Values)
                  {
                        for (var i = 0; i < channel.Subscribers.Count; i++)
                        {
                              if (channel.Subscribers[i].IsGhost)
                              {
                                    total++;
                              }
                        }
                  }

                  GhostCount = total;

                  return total;
            }

            private static void Hook()
            {
                  EchoInstrumentation.OnDispatchStart += HandleDispatchStart;
                  EchoInstrumentation.OnDispatchEnd += HandleDispatchEnd;
                  EchoInstrumentation.OnSubscribe += HandleSubscribe;
                  EchoInstrumentation.OnUnsubscribe += HandleUnsubscribe;
                  EchoInstrumentation.OnHandlerInvoked += HandleHandlerInvoked;
            }

            private static void OnPlayModeStateChanged(PlayModeStateChange change)
            {
                  if (change == PlayModeStateChange.EnteredPlayMode)
                  {
                        if (EchoMonitorSettings.AutoOpenOnPlay)
                        {
                              UI.EchoMonitorWindow.Open();
                        }
                  }

                  if (change == PlayModeStateChange.EnteredEditMode)
                  {
                        CountGhosts();
                        ModelChanged?.Invoke();
                  }
            }

            private static ProfilerMarker GetMarker(Type eventType)
            {
                  if (!Markers.TryGetValue(eventType, out ProfilerMarker marker))
                  {
                        marker = new ProfilerMarker($"Echo.{eventType.Name}");
                        Markers[eventType] = marker;
                  }

                  return marker;
            }

#region Instrumentation callbacks

            private static void HandleDispatchStart(Type eventType, object payload)
            {
                  if (!IsLive)
                  {
                        return;
                  }

                  ProfilerMarker.AutoScope scope = GetMarker(eventType).Auto();

                  PendingDispatches.Push(new PendingDispatch(
                        eventType,
                        CapturePayloads ? payload : null,
                        DateTime.UtcNow,
                        scope
                  ));
            }

            private static void HandleDispatchEnd(Type eventType, long elapsedTicks)
            {
                  if (!IsLive)
                  {
                        return;
                  }

                  if (PendingDispatches.Count == 0)
                  {
                        return;
                  }

                  PendingDispatch pending = PendingDispatches.Pop();
                  pending.Scope.Dispose();

                  if (pending.EventType != eventType)
                  {
                        PendingDispatches.Clear();

                        return;
                  }

                  DispatchRecord record = new(
                        eventType,
                        pending.Payload,
                        pending.Timestamp,
                        elapsedTicks,
                        Time.frameCount
                  );

                  Pending.Enqueue(new DispatchChange(record));
            }

            private static void HandleSubscribe(Type eventType, Delegate handler, SubscriptionKind kind)
            {
                  Pending.Enqueue(new SubscribeChange(eventType, handler, kind));
            }

            private static void HandleUnsubscribe(Type eventType, Delegate handler)
            {
                  Pending.Enqueue(new UnsubscribeChange(eventType, handler));
            }

            private static void HandleHandlerInvoked(Type eventType, Delegate handler)
            {
                  Pending.Enqueue(new HitChange(eventType, handler));
            }

#endregion

#region Flush

            private static void FlushPending()
            {
                  if (Pending.IsEmpty)
                  {
                        return;
                  }

                  var anyChange = false;

                  while (Pending.TryDequeue(out IPendingChange change))
                  {
                        anyChange |= change.Apply();
                  }

                  if (anyChange)
                  {
                        ModelChanged?.Invoke();
                  }
            }

            private static ChannelModel GetOrCreateChannel(Type eventType)
            {
                  if (!Channels.TryGetValue(eventType, out ChannelModel model))
                  {
                        model = new ChannelModel(eventType);
                        Channels.Add(eventType, model);
                        ChannelDiscovered?.Invoke(model);
                  }

                  return model;
            }

#endregion

#region Change records

            private interface IPendingChange
            {
                  bool Apply();
            }

            private readonly struct DispatchChange : IPendingChange
            {
                  private readonly DispatchRecord _record;
                  public DispatchChange(DispatchRecord record) => _record = record;

                  public bool Apply()
                  {
                        ChannelModel model = GetOrCreateChannel(_record.EventType);
                        model.RecordDispatch(in _record);
                        GlobalFeed.Push(_record);
                        TotalDispatches++;
                        DispatchObserved?.Invoke(_record);

                        return true;
                  }
            }

            private readonly struct SubscribeChange : IPendingChange
            {
                  private readonly Type _eventType;
                  private readonly Delegate _handler;
                  private readonly SubscriptionKind _kind;

                  public SubscribeChange(Type eventType, Delegate handler, SubscriptionKind kind)
                  {
                        _eventType = eventType;
                        _handler = handler;
                        _kind = kind;
                  }

                  public bool Apply()
                  {
                        ChannelModel model = GetOrCreateChannel(_eventType);
                        model.AddSubscriber(new SubscriberInfo(_eventType, _handler, _kind));

                        return true;
                  }
            }

            private readonly struct UnsubscribeChange : IPendingChange
            {
                  private readonly Type _eventType;
                  private readonly Delegate _handler;

                  public UnsubscribeChange(Type eventType, Delegate handler)
                  {
                        _eventType = eventType;
                        _handler = handler;
                  }

                  public bool Apply()
                  {
                        if (Channels.TryGetValue(_eventType, out ChannelModel model))
                        {
                              model.RemoveSubscriber(_handler);

                              return true;
                        }

                        return false;
                  }
            }

            private readonly struct HitChange : IPendingChange
            {
                  private readonly Type _eventType;
                  private readonly Delegate _handler;

                  public HitChange(Type eventType, Delegate handler)
                  {
                        _eventType = eventType;
                        _handler = handler;
                  }

                  public bool Apply()
                  {
                        if (!Channels.TryGetValue(_eventType, out ChannelModel model))
                        {
                              return false;
                        }

                        for (var i = 0; i < model.Subscribers.Count; i++)
                        {
                              SubscriberInfo info = model.Subscribers[i];

                              if (info.TryGetDelegate(out Delegate d) && ReferenceEquals(d, _handler))
                              {
                                    info.Hits++;

                                    return false;
                              }
                        }

                        return false;
                  }
            }

#endregion
      }
}