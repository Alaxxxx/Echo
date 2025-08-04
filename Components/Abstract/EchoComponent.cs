using UnityEngine;

namespace Echo.Components.Abstract
{
      public abstract class EchoComponent : MonoBehaviour
      {
            protected int MyId { get; private set; }

            protected virtual void Awake()
            {
                  MyId = gameObject.GetInstanceID();
            }
      }
}