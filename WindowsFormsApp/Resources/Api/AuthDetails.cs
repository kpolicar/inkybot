using System;

namespace WindowsFormsApp.Resources.Api
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
