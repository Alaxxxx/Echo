using UnityEditor;
using UnityEngine;

namespace Echo.Components.Editor
{
      [CustomEditor(typeof(EchoTrigger))]
      public sealed class EchoTriggerEditor : UnityEditor.Editor
      {
            public override void OnInspectorGUI()
            {
                  DrawDefaultInspector();

                  var trigger = (EchoTrigger)target;

                  if (!trigger.GetComponent<EchoPublisher>() && !trigger.GetComponent<EchoPublisher>())
                  {
                        EditorGUILayout.HelpBox("EchoTrigger requires an EchoPublisher component to function.", MessageType.Warning);
                  }

                  SerializedProperty triggerType = serializedObject.FindProperty("triggerType");
                  var triggerTypeEnum = (EchoTrigger.TriggerType)triggerType.enumValueIndex;

                  switch (triggerTypeEnum)
                  {
                        case EchoTrigger.TriggerType.OnCollisionEnter:
                        case EchoTrigger.TriggerType.OnCollisionExit:
                              if (!trigger.GetComponent<Collider>() && !trigger.GetComponent<Rigidbody>())
                              {
                                    EditorGUILayout.HelpBox("Collision triggers require a Collider or Rigidbody component.", MessageType.Info);
                              }

                              break;
                        case EchoTrigger.TriggerType.OnTriggerEnter:
                        case EchoTrigger.TriggerType.OnTriggerExit:
                              var collider = trigger.GetComponent<Collider>();

                              if (!collider)
                              {
                                    EditorGUILayout.HelpBox("Trigger events require a Collider component.", MessageType.Info);
                              }
                              else if (!collider.isTrigger)
                              {
                                    EditorGUILayout.HelpBox("Collider should have 'Is Trigger' enabled.", MessageType.Warning);
                              }

                              break;
                        case EchoTrigger.TriggerType.OnMouseDown:
                        case EchoTrigger.TriggerType.OnMouseUp:
                              if (!trigger.GetComponent<Collider>())
                              {
                                    EditorGUILayout.HelpBox("Mouse events require a Collider component.", MessageType.Info);
                              }

                              break;
                  }

                  if (Application.isPlaying && triggerTypeEnum == EchoTrigger.TriggerType.Custom)
                  {
                        EditorGUILayout.Space();

                        if (GUILayout.Button("Trigger Manually"))
                        {
                              trigger.TriggerManually();
                        }
                  }
            }
      }
}