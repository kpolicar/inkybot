using System;

namespace Inkybot.Api.Resources
{
    #pragma warning disable 8618
    [Serializable]
    public class AuthDetails
    {
        public string access_token;
        public int expires_in;
        public string refresh_token;
        public string token_type;
    }
    #pragma warning restore 8618
}
