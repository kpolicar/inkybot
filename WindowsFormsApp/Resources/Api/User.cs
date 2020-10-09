using System;

namespace WindowsFormsApp.Resources.Api
{
    [Serializable]
    public class User
    {
        public string name;
        public string email;
        public string password;
        public bool is_subscribed;
        public DateTime? subscribed_to;
    }
}