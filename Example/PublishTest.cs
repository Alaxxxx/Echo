using Echo.Example.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echo.Example
{
      public sealed class PublishTest : MonoBehaviour
      {
            private void Update()
            {
                  if (Keyboard.current.spaceKey.wasPressedThisFrame)
                  {
                        EventBus.Publish(new PlayerDamagedEvent { Health = 10f });
                  }
            }
      }
}