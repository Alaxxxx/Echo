using Echo.Components.Enum;
using UnityEngine;

namespace Echo.Components.Data
{
      [System.Serializable]
      public class EventFieldConfig
      {
            public string fieldName;
            public EventFieldType fieldType;

            [Header("Values")]
            public int intValue;
            public float floatValue;
            public string stringValue;
            public bool boolValue;
            public Vector3 vector3Value;
            public GameObject gameObjectValue;
      }
}