
using System;
using System.Reflection;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        [Serializable]
        public class Release
        {
            public string id;
            public string gameName;
            public string location;
            public string ExeLocation => location + @"/Dofus.exe";
        }
    }
}
