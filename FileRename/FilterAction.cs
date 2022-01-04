namespace FileRename
{
    public class FilterAction : IEvent
    {
        public string Value { get; set; }
        public string Properties { get; set; }

        public FilterAction(string value, string properties)
        {
            Value = value;
            Properties = properties;
        }
    }
}

