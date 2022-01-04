using System.IO;
using System.Text.RegularExpressions;

namespace FileRename
{
    public class ChangeItem : PropertySensitive
    {
        public Match Match { get => GetPar<Match>(); set => SetPar<Match>(value); }
        public string Source { get => GetPar<string>(); set => SetPar<string>(value); }
        public string Destination { get => GetPar<string>(); set => SetPar<string>(value); }
        public bool Moved { get; set; } = false;



        public ChangeItem(string file, Match match)
        {
            Source = file;
            Match = match;
        }

        public override string ToString()
        {
            return Source;
        }

    }

}

