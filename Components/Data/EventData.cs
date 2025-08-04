using System.Collections.Generic;

namespace Echo.Components.Data
{
      [System.Serializable]
      public class EventData
      {
            public string eventType;
            public int sourceId;
            public int targetId;
            public Dictionary<string, object> Fields = new();

            public T GetField<T>(string fieldName, T defaultValue = default)
            {
                  if (Fields.TryGetValue(fieldName, out object value) && value is T typedValue)
                  {
                        return typedValue;
                  }

                  return defaultValue;
            }
      }
}