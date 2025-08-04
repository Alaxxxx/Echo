using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Echo.Interface;
using UnityEditor;
using UnityEngine;

namespace Echo.Components.Editor
{
      [CustomEditor(typeof(EchoSubscriber))]
      public sealed class EchoSubscriberEditor : UnityEditor.Editor
      {
            private SerializedProperty subscriptionConfigs;
            private SerializedProperty subscribeOnEnable;
            private SerializedProperty unsubscribeOnDisable;

            private string[] availableEventTypes;
            private bool showEventTypes;

            private void OnEnable()
            {
                  subscriptionConfigs = serializedObject.FindProperty("subscriptionConfigs");
                  subscribeOnEnable = serializedObject.FindProperty("subscribeOnEnable");
                  unsubscribeOnDisable = serializedObject.FindProperty("unsubscribeOnDisable");

                  RefreshAvailableEventTypes();
            }

            public override void OnInspectorGUI()
            {
                  serializedObject.Update();

                  // Header
                  EditorGUILayout.Space();
                  var headerStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
                  EditorGUILayout.LabelField("Echo Event Subscriber", headerStyle);
                  EditorGUILayout.Space();

                  // Subscription options
                  EditorGUILayout.LabelField("Subscription Options", EditorStyles.boldLabel);
                  EditorGUILayout.PropertyField(subscribeOnEnable);
                  EditorGUILayout.PropertyField(unsubscribeOnDisable);
                  EditorGUILayout.Space();

                  // Subscription configurations
                  EditorGUILayout.LabelField("Event Subscriptions", EditorStyles.boldLabel);

                  // Show available event types button
                  if (GUILayout.Button("Show Available Event Types"))
                  {
                        showEventTypes = !showEventTypes;
                  }

                  if (showEventTypes)
                  {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("Available Event Types:", EditorStyles.miniLabel);

                        foreach (string eventType in availableEventTypes)
                        {
                              if (GUILayout.Button(eventType, EditorStyles.miniButton))
                              {
                                    AddSubscriptionConfig(eventType);
                              }
                        }

                        EditorGUILayout.EndVertical();
                  }

                  for (int i = 0; i < subscriptionConfigs.arraySize; i++)
                  {
                        SerializedProperty config = subscriptionConfigs.GetArrayElementAtIndex(i);
                        DrawSubscriptionConfig(config, i);
                  }

                  EditorGUILayout.BeginHorizontal();

                  if (GUILayout.Button("Add Subscription"))
                  {
                        subscriptionConfigs.arraySize++;
                        SerializedProperty newConfig = subscriptionConfigs.GetArrayElementAtIndex(subscriptionConfigs.arraySize - 1);
                        ResetSubscriptionConfig(newConfig);
                  }

                  GUI.enabled = subscriptionConfigs.arraySize > 0;

                  if (GUILayout.Button("Remove Last"))
                  {
                        subscriptionConfigs.arraySize--;
                  }

                  GUI.enabled = true;
                  EditorGUILayout.EndHorizontal();

                  if (Application.isPlaying)
                  {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Active Subscriptions: {subscriptionConfigs.arraySize}");
                  }

                  serializedObject.ApplyModifiedProperties();
            }

            private void DrawSubscriptionConfig(SerializedProperty config, int index)
            {
                  EditorGUILayout.BeginVertical("box");

                  SerializedProperty eventTypeName = config.FindPropertyRelative("eventTypeName");
                  SerializedProperty isTrackedEvent = config.FindPropertyRelative("isTrackedEvent");
                  SerializedProperty filterByTarget = config.FindPropertyRelative("filterByTarget");
                  SerializedProperty filterBySource = config.FindPropertyRelative("filterBySource");

                  config.isExpanded = EditorGUILayout.Foldout(config.isExpanded, $"Subscription {index + 1}: {eventTypeName.stringValue}");

                  if (config.isExpanded)
                  {
                        EditorGUI.indentLevel++;

                        int currentIndex = Array.IndexOf(availableEventTypes, eventTypeName.stringValue);
                        int newIndex = EditorGUILayout.Popup("Event Type", currentIndex, availableEventTypes);

                        if (newIndex >= 0 && newIndex < availableEventTypes.Length)
                        {
                              eventTypeName.stringValue = availableEventTypes[newIndex];

                              Type eventType = GetEventType(availableEventTypes[newIndex]);

                              if (eventType != null)
                              {
                                    isTrackedEvent.boolValue = typeof(ITrackedEvent).IsAssignableFrom(eventType);
                              }
                        }

                        EditorGUILayout.PropertyField(isTrackedEvent);

                        if (isTrackedEvent.boolValue)
                        {
                              EditorGUI.indentLevel++;

                              EditorGUILayout.PropertyField(filterByTarget);

                              if (filterByTarget.boolValue)
                              {
                                    SerializedProperty useCurrentAsTarget = config.FindPropertyRelative("useCurrentAsTarget");
                                    SerializedProperty targetFilter = config.FindPropertyRelative("targetFilter");

                                    EditorGUILayout.PropertyField(useCurrentAsTarget, new GUIContent("Use This GameObject"));

                                    if (!useCurrentAsTarget.boolValue)
                                    {
                                          EditorGUILayout.PropertyField(targetFilter, new GUIContent("Target Filter"));
                                    }
                              }

                              EditorGUILayout.PropertyField(filterBySource);

                              if (filterBySource.boolValue)
                              {
                                    SerializedProperty useCurrentAsSource = config.FindPropertyRelative("useCurrentAsSource");
                                    SerializedProperty sourceFilter = config.FindPropertyRelative("sourceFilter");

                                    EditorGUILayout.PropertyField(useCurrentAsSource, new GUIContent("Use This GameObject"));

                                    if (!useCurrentAsSource.boolValue)
                                    {
                                          EditorGUILayout.PropertyField(sourceFilter, new GUIContent("Source Filter"));
                                    }
                              }

                              EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Response Events", EditorStyles.boldLabel);

                        SerializedProperty onEventReceived = config.FindPropertyRelative("OnEventReceived");
                        SerializedProperty onSimpleEventReceived = config.FindPropertyRelative("OnSimpleEventReceived");

                        EditorGUILayout.PropertyField(onEventReceived, new GUIContent("On Event Received (with data)"));
                        EditorGUILayout.PropertyField(onSimpleEventReceived, new GUIContent("On Event Received (simple)"));

                        EditorGUILayout.Space();
                        GUI.color = Color.red;

                        if (GUILayout.Button("Remove This Subscription"))
                        {
                              subscriptionConfigs.DeleteArrayElementAtIndex(index);
                        }

                        GUI.color = Color.white;

                        EditorGUI.indentLevel--;
                  }

                  EditorGUILayout.EndVertical();
                  EditorGUILayout.Space();
            }

            private void RefreshAvailableEventTypes()
            {
                  var eventTypes = new List<string>();

                  foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                  {
                        try
                        {
                              IOrderedEnumerable<string> types = assembly.GetTypes()
                                                                         .Where(static t => typeof(IEvent).IsAssignableFrom(t) && t.IsValueType && !t.IsAbstract)
                                                                         .Select(static t => t.FullName)
                                                                         .OrderBy(static eventName => eventName);

                              eventTypes.AddRange(types);
                        }
                        catch
                        {
                              // Skip assemblies that can't be loaded
                        }
                  }

                  availableEventTypes = eventTypes.ToArray();
            }

            private void AddSubscriptionConfig(string eventTypeName)
            {
                  subscriptionConfigs.arraySize++;
                  SerializedProperty newConfig = subscriptionConfigs.GetArrayElementAtIndex(subscriptionConfigs.arraySize - 1);
                  ResetSubscriptionConfig(newConfig);
                  newConfig.FindPropertyRelative("eventTypeName").stringValue = eventTypeName;

                  Type eventType = GetEventType(eventTypeName);

                  if (eventType != null)
                  {
                        newConfig.FindPropertyRelative("isTrackedEvent").boolValue = typeof(ITrackedEvent).IsAssignableFrom(eventType);
                  }
            }

            private static void ResetSubscriptionConfig(SerializedProperty config)
            {
                  config.FindPropertyRelative("eventTypeName").stringValue = "";
                  config.FindPropertyRelative("isTrackedEvent").boolValue = false;
                  config.FindPropertyRelative("filterByTarget").boolValue = false;
                  config.FindPropertyRelative("filterBySource").boolValue = false;
                  config.FindPropertyRelative("useCurrentAsTarget").boolValue = true;
                  config.FindPropertyRelative("useCurrentAsSource").boolValue = false;
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
      }
}