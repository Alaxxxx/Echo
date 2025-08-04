using System.Collections.Generic;
using UnityEngine;

namespace Echo.Components.Data
{
      [System.Serializable]
      public class EventPublisherConfig
      {
            [Header("Event Configuration")]
            public string eventTypeName;
            public bool isTrackedEvent;
            public bool hasTargetObject;

            [Header("Target Settings")]
            [SerializeField] private GameObject targetObject;
            [SerializeField] private bool useCurrentGameObject;

            [Header("Data Fields")]
            public List<EventFieldConfig> fields = new();

            public GameObject GetTarget(GameObject fallback)
            {
                  if (useCurrentGameObject)
                  {
                        return fallback;
                  }

                  return targetObject ? targetObject : fallback;
            }
      }
}