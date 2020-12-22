using System;

namespace Inkybot.Api.Resources
{
    [Serializable]
    public class User
    {
        public string email;
        public bool is_subscribed;
        public bool is_free_trial;
        public bool free_trial_available;
        public string name;
        public string password;
        public DateTime? subscribed_to;
    }
}
