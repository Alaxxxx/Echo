using System;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Unity.Data
{
      public readonly struct FilteredSubscription<T> : IDisposable where T : struct, IEvent
      {
            private readonly int _id;

            internal FilteredSubscription(int id)
            {
                  _id = id;
            }

            public void Dispose()
            {
                  EventChannel<T>.RemoveFilteredHandler(_id);
            }
      }
}