namespace FileRename
{
    public class RenameAction : IEvent
    {
        public string Value { get; set; }

        public RenameAction(string value)
        {
            Value = value;
        }
    }
}

