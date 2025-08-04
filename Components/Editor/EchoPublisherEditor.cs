using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Echo.Components.Enum;
using Echo.Interface;
using UnityEditor;
using UnityEngine;

namespace Echo.Components.Editor
{
      [CustomEditor(typeof(EchoPublisher))]
      public sealed class EchoPublisherEditor : UnityEditor.Editor
      {
            private SerializedProperty eventConfigs;
            private SerializedProperty publishOnStart;
            private SerializedProperty publishOnEnable;
            private SerializedProperty autoPublishInterval;

            private string[] availableEventTypes;
            private bool showEventTypes;

            private void OnEnable()
            {
                  eventConfigs = serializedObject.FindProperty("eventConfigs");
                  publishOnStart = serializedObject.FindProperty("publishOnStart");
                  publishOnEnable = serializedObject.FindProperty("publishOnEnable");
                  autoPublishInterval = serializedObject.FindProperty("autoPublishInterval");

                  RefreshAvailableEventTypes();
            }

            public override void OnInspectorGUI()
            {
                  serializedObject.Update();

                  // Header
                  EditorGUILayout.Space();
                  var headerStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
                  EditorGUILayout.LabelField("Echo Event Publisher", headerStyle);
                  EditorGUILayout.Space();

                  // Publishing options
                  EditorGUILayout.LabelField("Publishing Options", EditorStyles.boldLabel);
                  EditorGUILayout.PropertyField(publishOnStart);
                  EditorGUILayout.PropertyField(publishOnEnable);
                  EditorGUILayout.PropertyField(autoPublishInterval);

                  if (autoPublishInterval.floatValue > 0)
                  {
                        EditorGUILayout.HelpBox("Auto-publishing every " + autoPublishInterval.floatValue + " seconds", MessageType.Info);
                  }

                  EditorGUILayout.Space();

                  EditorGUILayout.LabelField("Event Configurations", EditorStyles.boldLabel);

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
                                    AddEventConfig(eventType);
                              }
                        }

                        EditorGUILayout.EndVertical();
                  }

                  for (int i = 0; i < eventConfigs.arraySize; i++)
                  {
                        SerializedProperty config = eventConfigs.GetArrayElementAtIndex(i);
                        DrawEventConfig(config, i);
                  }

                  EditorGUILayout.BeginHorizontal();

                  if (GUILayout.Button("Add Event Configuration"))
                  {
                        eventConfigs.arraySize++;
                        SerializedProperty newConfig = eventConfigs.GetArrayElementAtIndex(eventConfigs.arraySize - 1);
                        ResetEventConfig(newConfig);
                  }

                  GUI.enabled = eventConfigs.arraySize > 0;

                  if (GUILayout.Button("Remove Last"))
                  {
                        eventConfigs.arraySize--;
                  }

                  GUI.enabled = true;
                  EditorGUILayout.EndHorizontal();

                  if (Application.isPlaying)
                  {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                        if (GUILayout.Button("Publish All Events"))
                        {
                              ((EchoPublisher)target).PublishAllEvents();
                        }
                  }

                  serializedObject.ApplyModifiedProperties();
            }

            private void DrawEventConfig(SerializedProperty config, int index)
            {
                  EditorGUILayout.BeginVertical("box");

                  SerializedProperty eventTypeName = config.FindPropertyRelative("eventTypeName");
                  SerializedProperty isTrackedEvent = config.FindPropertyRelative("isTrackedEvent");
                  SerializedProperty hasTargetObject = config.FindPropertyRelative("hasTargetObject");
                  SerializedProperty fields = config.FindPropertyRelative("fields");

                  SerializedProperty foldoutProperty = config.FindPropertyRelative("foldout");

                  if (foldoutProperty == null)
                  {
                        config.isExpanded = EditorGUILayout.Foldout(config.isExpanded, $"Event {index + 1}: {eventTypeName.stringValue}");
                  }

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
                              EditorGUILayout.PropertyField(hasTargetObject);

                              if (hasTargetObject.boolValue)
                              {
                                    SerializedProperty targetObject = config.FindPropertyRelative("targetObject");
                                    SerializedProperty useCurrentGameObject = config.FindPropertyRelative("useCurrentGameObject");

                                    EditorGUILayout.PropertyField(useCurrentGameObject);

                                    if (!useCurrentGameObject.boolValue)
                                    {
                                          EditorGUILayout.PropertyField(targetObject);
                                    }
                              }

                              EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Event Fields", EditorStyles.boldLabel);

                        for (int j = 0; j < fields.arraySize; j++)
                        {
                              DrawEventField(fields.GetArrayElementAtIndex(j));
                        }

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Add Field"))
                        {
                              fields.arraySize++;
                        }

                        if (fields.arraySize > 0 && GUILayout.Button("Remove Field"))
                        {
                              fields.arraySize--;
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        GUI.color = Color.red;

                        if (GUILayout.Button("Remove This Event Configuration"))
                        {
                              eventConfigs.DeleteArrayElementAtIndex(index);
                        }

                        GUI.color = Color.white;

                        EditorGUI.indentLevel--;
                  }

                  EditorGUILayout.EndVertical();
                  EditorGUILayout.Space();
            }

            private static void DrawEventField(SerializedProperty field)
            {
                  EditorGUILayout.BeginVertical("box");

                  SerializedProperty fieldName = field.FindPropertyRelative("fieldName");
                  SerializedProperty fieldType = field.FindPropertyRelative("fieldType");

                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.PropertyField(fieldName, GUIContent.none, GUILayout.Width(120));
                  EditorGUILayout.PropertyField(fieldType, GUIContent.none, GUILayout.Width(80));

                  var fieldTypeEnum = (EventFieldType)fieldType.enumValueIndex;

                  switch (fieldTypeEnum)
                  {
                        case EventFieldType.Int:
                              SerializedProperty intValue = field.FindPropertyRelative("intValue");
                              EditorGUILayout.PropertyField(intValue, GUIContent.none);

                              break;
                        case EventFieldType.Float:
                              SerializedProperty floatValue = field.FindPropertyRelative("floatValue");
                              EditorGUILayout.PropertyField(floatValue, GUIContent.none);

                              break;
                        case EventFieldType.String:
                              SerializedProperty stringValue = field.FindPropertyRelative("stringValue");
                              EditorGUILayout.PropertyField(stringValue, GUIContent.none);

                              break;
                        case EventFieldType.Bool:
                              SerializedProperty boolValue = field.FindPropertyRelative("boolValue");
                              EditorGUILayout.PropertyField(boolValue, GUIContent.none);

                              break;
                        case EventFieldType.Vector3:
                              SerializedProperty vector3Value = field.FindPropertyRelative("vector3Value");
                              EditorGUILayout.PropertyField(vector3Value, GUIContent.none);

                              break;
                        case EventFieldType.GameObject:
                              SerializedProperty gameObjectValue = field.FindPropertyRelative("gameObjectValue");
                              EditorGUILayout.PropertyField(gameObjectValue, GUIContent.none);

                              break;
                  }

                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.EndVertical();
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

            private void AddEventConfig(string eventTypeName)
            {
                  eventConfigs.arraySize++;
                  SerializedProperty newConfig = eventConfigs.GetArrayElementAtIndex(eventConfigs.arraySize - 1);
                  ResetEventConfig(newConfig);
                  newConfig.FindPropertyRelative("eventTypeName").stringValue = eventTypeName;

                  Type eventType = GetEventType(eventTypeName);

                  if (eventType != null)
                  {
                        newConfig.FindPropertyRelative("isTrackedEvent").boolValue = typeof(ITrackedEvent).IsAssignableFrom(eventType);
                  }
            }

            private static void ResetEventConfig(SerializedProperty config)
            {
                  config.FindPropertyRelative("eventTypeName").stringValue = "";
                  config.FindPropertyRelative("isTrackedEvent").boolValue = false;
                  config.FindPropertyRelative("hasTargetObject").boolValue = false;
                  config.FindPropertyRelative("fields").arraySize = 0;
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