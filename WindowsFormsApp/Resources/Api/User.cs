using System;

namespace WindowsFormsApp.Resources.Api
{
    [Serializable]
    public class User
    {
        public string email;
        public bool is_subscribed;
        public string name;
        public string password;
        public DateTime? subscribed_to;
    }
}
