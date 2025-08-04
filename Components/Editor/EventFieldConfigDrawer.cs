using Echo.Components.Data;
using Echo.Components.Enum;
using UnityEditor;
using UnityEngine;

namespace Echo.Components.Editor
{
      [CustomPropertyDrawer(typeof(EventFieldConfig))]
      public class EventFieldConfigDrawer : PropertyDrawer
      {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                  EditorGUI.BeginProperty(position, label, property);

                  SerializedProperty fieldName = property.FindPropertyRelative("fieldName");
                  SerializedProperty fieldType = property.FindPropertyRelative("fieldType");

                  var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                  var nameRect = new Rect(rect.x, rect.y, rect.width * 0.3f, rect.height);
                  var typeRect = new Rect(rect.x + rect.width * 0.3f + 5, rect.y, rect.width * 0.25f, rect.height);
                  var valueRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height);

                  EditorGUI.PropertyField(nameRect, fieldName, GUIContent.none);
                  EditorGUI.PropertyField(typeRect, fieldType, GUIContent.none);

                  var fieldTypeEnum = (EventFieldType)fieldType.enumValueIndex;

                  switch (fieldTypeEnum)
                  {
                        case EventFieldType.Int:
                              EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("intValue"), GUIContent.none);

                              break;
                        case EventFieldType.Float:
                              EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("floatValue"), GUIContent.none);

                              break;
                        case EventFieldType.String:
                              EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), GUIContent.none);

                              break;
                        case EventFieldType.Bool:
                              EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boolValue"), GUIContent.none);

                              break;
                        case EventFieldType.Vector3:
                              rect.y += EditorGUIUtility.singleLineHeight + 2;
                              var vector3Rect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                              EditorGUI.PropertyField(vector3Rect, property.FindPropertyRelative("vector3Value"), GUIContent.none);

                              break;
                        case EventFieldType.GameObject:
                              EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("gameObjectValue"), GUIContent.none);

                              break;
                  }

                  EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                  SerializedProperty fieldType = property.FindPropertyRelative("fieldType");
                  var fieldTypeEnum = (EventFieldType)fieldType.enumValueIndex;

                  return fieldTypeEnum == EventFieldType.Vector3 ? EditorGUIUtility.singleLineHeight * 2 + 2 : EditorGUIUtility.singleLineHeight;
            }
      }
}