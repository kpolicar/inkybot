using System;

namespace Inkybot.Api.Resources
{
    #pragma warning disable 8618
    [Serializable]
    public class VersionDetails
    {
        public string name;
        public string endpoint;
        public string number;
    }
    #pragma warning restore 8618
}
