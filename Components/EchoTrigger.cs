using Echo.Components.Abstract;
using UnityEngine;

namespace Echo.Components
{
      [AddComponentMenu("Echo/Event Trigger")]
      public class EchoTrigger : EchoComponent
      {
            [Header("Trigger Settings")]
            [SerializeField] private EchoPublisher publisher;
            [SerializeField] private TriggerType triggerType = TriggerType.OnCollisionEnter;
            [SerializeField] private string requiredTag = "";
            [SerializeField] private LayerMask layerMask = -1;

            public enum TriggerType
            {
                  OnCollisionEnter,
                  OnCollisionExit,
                  OnTriggerEnter,
                  OnTriggerExit,
                  OnMouseDown,
                  OnMouseUp,
                  Custom
            }

            private void Start()
            {
                  publisher = publisher ? publisher : GetComponent<EchoPublisher>();
            }

            private void OnCollisionEnter(Collision collision)
            {
                  if (triggerType == TriggerType.OnCollisionEnter)
                  {
                        HandleTrigger(collision.gameObject);
                  }
            }

            private void OnCollisionExit(Collision collision)
            {
                  if (triggerType == TriggerType.OnCollisionExit)
                  {
                        HandleTrigger(collision.gameObject);
                  }
            }

            private void OnTriggerEnter(Collider other)
            {
                  if (triggerType == TriggerType.OnTriggerEnter)
                  {
                        HandleTrigger(other.gameObject);
                  }
            }

            private void OnTriggerExit(Collider other)
            {
                  if (triggerType == TriggerType.OnTriggerExit)
                  {
                        HandleTrigger(other.gameObject);
                  }
            }

            private void OnMouseDown()
            {
                  if (triggerType == TriggerType.OnMouseDown)
                  {
                        HandleTrigger(gameObject);
                  }
            }

            private void OnMouseUp()
            {
                  if (triggerType == TriggerType.OnMouseUp)
                  {
                        HandleTrigger(gameObject);
                  }
            }

            public void TriggerManually()
            {
                  HandleTrigger(gameObject);
            }

            private void HandleTrigger(GameObject triggeringObject)
            {
                  if (!string.IsNullOrEmpty(requiredTag) && !triggeringObject.CompareTag(requiredTag))
                  {
                        return;
                  }

                  if (layerMask != -1 && (layerMask & (1 << triggeringObject.layer)) == 0)
                  {
                        return;
                  }

                  if (publisher)
                  {
                        publisher.PublishAllEvents();
                  }
            }
      }
}