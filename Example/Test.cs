using Echo.Example.Events;
using UnityEngine;

namespace Echo.Example
{
      public sealed class Test : MonoBehaviour
      {
            [SerializeField] private float health = 100f;

            private void OnEnable()
            {
                  EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            }

            private void OnDisable()
            {
                  EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            }

            private void OnPlayerDamaged(PlayerDamagedEvent playerDamagedEvent)
            {
                  health -= playerDamagedEvent.Health;
                  Debug.Log($"Remaining health: {health}");
            }
      }
}