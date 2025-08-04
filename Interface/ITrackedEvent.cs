namespace Echo.Interface
{
      /// <summary>
      /// Represents an interface for events that are tracked with specific source and target identifiers
      /// in the Echo system. This interface extends the base <see cref="IEvent"/> interface, providing
      /// additional properties to specify the source and target entities involved in the event.
      /// </summary>
      public interface ITrackedEvent : IEvent
      {
            public int SourceId { get; set; }
            public int TargetId { get; set; }
      }
}