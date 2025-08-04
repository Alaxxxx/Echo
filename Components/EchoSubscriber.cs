using System;
using System.Collections.Generic;
using System.Reflection;
using Echo.Components.Abstract;
using Echo.Components.Data;
using Echo.Core;
using Echo.Interface;
using UnityEngine;

namespace Echo.Components
{
      [AddComponentMenu("Echo/Event Subscriber")]
      public class EchoSubscriber : EchoComponent
      {
            [Header("Subscriber Configuration")]
            [SerializeField] private List<EventSubscriberConfig> subscriptionConfigs = new();

            [Header("Global Settings")]
            [SerializeField] private bool subscribeOnEnable = true;
            [SerializeField] private bool unsubscribeOnDisable = true;

            private readonly Dictionary<Type, Delegate> activeSubscriptions = new();

            protected override void Awake()
            {
                  base.Awake();
                  ValidateConfigurations();
            }

            private void OnEnable()
            {
                  if (subscribeOnEnable)
                  {
                        SubscribeToAllEvents();
                  }
            }

            private void OnDisable()
            {
                  if (unsubscribeOnDisable)
                  {
                        UnsubscribeFromAllEvents();
                  }
            }

            private void OnDestroy()
            {
                  UnsubscribeFromAllEvents();
            }

            public void SubscribeToAllEvents()
            {
                  foreach (EventSubscriberConfig config in subscriptionConfigs)
                  {
                        SubscribeToEvent(config);
                  }
            }

            public void UnsubscribeFromAllEvents()
            {
                  foreach (KeyValuePair<Type, Delegate> kvp in activeSubscriptions)
                  {
                        MethodInfo unsubscribeMethod = typeof(EventBus).GetMethod("Unsubscribe").MakeGenericMethod(kvp.Key);
                        unsubscribeMethod.Invoke(null, new object[] { kvp.Value });
                  }

                  activeSubscriptions.Clear();
            }

            private void SubscribeToEvent(EventSubscriberConfig config)
            {
                  Type eventType = GetEventType(config.eventTypeName);

                  if (eventType == null)
                  {
                        Debug.LogError($"[Echo] Event type not found: {config.eventTypeName}");

                        return;
                  }

                  Type delegateType = typeof(Action<>).MakeGenericType(eventType);
                  MethodInfo handler = CreateEventHandler();
                  var typedHandler = Delegate.CreateDelegate(delegateType, this, handler);

                  MethodInfo subscribeMethod = typeof(EventBus).GetMethod("Subscribe").MakeGenericMethod(eventType);
                  subscribeMethod.Invoke(null, new object[] { typedHandler });

                  activeSubscriptions[eventType] = typedHandler;
            }

            private MethodInfo CreateEventHandler()
            {
                  return GetType().GetMethod("HandleEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            private Type GetEventType(string typeName)
            {
                  foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                  {
                        Type type = assembly.GetType(typeName);

                        if (type != null && typeof(IEvent).IsAssignableFrom(type))
                        {
                              return type;
                        }
                  }

                  return null;
            }

            private void ValidateConfigurations()
            {
                  foreach (EventSubscriberConfig config in subscriptionConfigs)
                  {
                        if (string.IsNullOrEmpty(config.eventTypeName))
                        {
                              Debug.LogWarning($"[Echo] Empty event type name in {this.gameObject.name}");
                        }
                  }
            }
      }
}