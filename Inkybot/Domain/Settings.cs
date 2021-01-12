using System;
using System.Reflection;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        [Serializable]
        public class Settings
        {
            public Language language;
        }
        
        [Serializable]
        public class Language
        {
            public string name;
            public string @default;
            public string value;
        }
    }
}
