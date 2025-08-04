using System;
using System.Collections.Generic;
using System.Reflection;
using Echo.Components.Abstract;
using Echo.Components.Data;
using Echo.Components.Enum;
using Echo.Core;
using Echo.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace Echo.Components
{
      [AddComponentMenu("Echo/Event Publisher")]
      public class EchoPublisher : EchoComponent
      {
            [Header("Publisher Configuration")]
            [SerializeField] private List<EventPublisherConfig> eventConfigs = new();

            [Header("Publishing Options")]
            [SerializeField] private bool publishOnEnable;

            [Header("Unity Events")]
            public UnityEvent onEventPublished;

            private float lastPublishTime;

            protected override void Awake()
            {
                  base.Awake();
                  ValidateConfigurations();
            }

            private void OnEnable()
            {
                  if (publishOnEnable)
                  {
                        PublishAllEvents();
                  }
            }

            public void PublishAllEvents()
            {
                  foreach (EventPublisherConfig config in eventConfigs)
                  {
                        PublishEvent(config);
                  }
            }

            public void PublishEvent(int configIndex)
            {
                  if (configIndex >= 0 && configIndex < eventConfigs.Count)
                  {
                        PublishEvent(eventConfigs[configIndex]);
                  }
            }

            public void PublishEvent(string eventTypeName)
            {
                  EventPublisherConfig config = eventConfigs.Find(c => c.eventTypeName == eventTypeName);

                  if (config != null)
                  {
                        PublishEvent(config);
                  }
            }

            private void PublishEvent(EventPublisherConfig config)
            {
                  try
                  {
                        object eventData = CreateEventFromConfig(config);

                        Type eventType = GetEventType(config.eventTypeName);

                        if (eventType != null)
                        {
                              MethodInfo publishMethod = typeof(EventBus).GetMethod("Publish").MakeGenericMethod(eventType);
                              publishMethod.Invoke(null, new[] { eventData });

                              onEventPublished?.Invoke();
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"[Echo] Failed to publish event {config.eventTypeName}: {e.Message}");
                  }
            }

            private object CreateEventFromConfig(EventPublisherConfig config)
            {
                  Type eventType = GetEventType(config.eventTypeName);
                  object eventInstance = Activator.CreateInstance(eventType);

                  if (config.isTrackedEvent)
                  {
                        PropertyInfo sourceIdProperty = eventType.GetProperty("SourceId");
                        sourceIdProperty?.SetValue(eventInstance, MyId);

                        if (config.hasTargetObject)
                        {
                              GameObject target = config.GetTarget(gameObject);
                              PropertyInfo targetIdProperty = eventType.GetProperty("TargetId");
                              targetIdProperty?.SetValue(eventInstance, target.GetInstanceID());
                        }
                  }

                  foreach (EventFieldConfig field in config.fields)
                  {
                        PropertyInfo property = eventType.GetProperty(field.fieldName);

                        if (property != null)
                        {
                              object value = field.fieldType switch
                              {
                                          EventFieldType.Int => field.intValue,
                                          EventFieldType.Float => field.floatValue,
                                          EventFieldType.String => field.stringValue,
                                          EventFieldType.Bool => field.boolValue,
                                          EventFieldType.Vector3 => field.vector3Value,
                                          EventFieldType.GameObject => field.gameObjectValue ? field.gameObjectValue.GetInstanceID() : -1,
                                          _ => null
                              };
                              property.SetValue(eventInstance, value);
                        }
                  }

                  return eventInstance;
            }

            private static Type GetEventType(string typeName)
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
                  foreach (EventPublisherConfig config in eventConfigs)
                  {
                        if (string.IsNullOrEmpty(config.eventTypeName))
                        {
                              Debug.LogWarning($"[Echo] Empty event type name in {gameObject.name}");
                        }
                  }
            }

            public void AddEventConfig()
            {
                  eventConfigs.Add(new EventPublisherConfig());
            }

            public void RemoveEventConfig(int index)
            {
                  if (index >= 0 && index < eventConfigs.Count)
                  {
                        eventConfigs.RemoveAt(index);
                  }
            }
      }
}