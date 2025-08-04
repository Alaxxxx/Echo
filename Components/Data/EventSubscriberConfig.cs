using Echo.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace Echo.Components.Data
{
      [System.Serializable]
      public class EventSubscriberConfig
      {
            [Header("Event Configuration")]
            public string eventTypeName;
            public bool isTrackedEvent;
            public bool filterByTarget;
            public bool filterBySource;

            [Header("Filtering")]
            [SerializeField] private GameObject sourceFilter;
            [SerializeField] private GameObject targetFilter;
            [SerializeField] private bool useCurrentAsTarget = true;
            [SerializeField] private bool useCurrentAsSource;

            [Header("Response")]
            public UnityEvent<EventData> OnEventReceived;
            public UnityEvent OnSimpleEventReceived;

            public bool ShouldAcceptEvent(ITrackedEvent trackedEvent, int myId)
            {
                  if (!isTrackedEvent)
                  {
                        return true;
                  }

                  if (filterByTarget)
                  {
                        int expectedTargetId = useCurrentAsTarget ? myId : (targetFilter != null ? targetFilter.GetInstanceID() : -1);

                        if (trackedEvent.TargetId != expectedTargetId && trackedEvent.TargetId != -1)
                        {
                              return false;
                        }
                  }

                  if (filterBySource)
                  {
                        int expectedSourceId = useCurrentAsSource ? myId : (sourceFilter != null ? sourceFilter.GetInstanceID() : -1);

                        if (trackedEvent.SourceId != expectedSourceId)
                        {
                              return false;
                        }
                  }

                  return true;
            }
      }
}