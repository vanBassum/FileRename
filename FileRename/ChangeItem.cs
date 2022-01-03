using System.IO;
using System.Text.RegularExpressions;

namespace FileRename
{
    public class ChangeItem : PropertySensitive
    {
        public Match Match { get => GetPar<Match>(); set => SetPar<Match>(value); }
        public FileInfo Source { get => GetPar<FileInfo>(); set => SetPar<FileInfo>(value); }

        public FileInfo Destination { get => GetPar<FileInfo>(); set => SetPar<FileInfo>(value); }


        public ChangeItem(string file, Match match)
        {
            Source = new FileInfo(file);
            Match = match;
        }

        public override string ToString()
        {
            return Source.FullName;
        }

    }

}

