using System;

namespace Inkybot.Resources.Api
{
    [Serializable]
    public class AuthDetails
    {
        public string access_token;
        public int expires_in;
        public string refresh_token;
        public string token_type;
    }
}
