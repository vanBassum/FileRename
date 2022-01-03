namespace FileRename
{
    public class FilterChangedEvent : IEvent
    { 
        public string Value { get; set; }

        public FilterChangedEvent(string value)
        {
            Value = value;
        }
    }
}

