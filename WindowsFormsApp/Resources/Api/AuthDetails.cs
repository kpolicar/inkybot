using System;

namespace WindowsFormsApp.Resources.Api
{
    [Serializable]
    public class AuthDetails
    {
        public string token_type;
        public int expires_in;
        public string access_token;
        public string refresh_token;
    }
}