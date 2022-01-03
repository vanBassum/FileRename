namespace FileRename
{
    public class DestinationChangedEvent : IEvent
    {
        public string Value { get; set; }

        public DestinationChangedEvent(string value)
        {
            Value = value;
        }
    }
}

