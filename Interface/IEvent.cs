namespace OpalStudio.Echo.Interface
{
      /// <summary>
      /// Represents the base interface for all event types in the Echo system.
      /// All events must be value types (structs) implementing this interface
      /// to guarantee zero-allocation publishing.
      /// </summary>
      public interface IEvent
      {
      }
}