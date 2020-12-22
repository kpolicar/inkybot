using System;

namespace Inkybot.Api.Resources
{
    [Serializable]
    public class FreeTrial
    {
        public bool expired;
        public DateTime? created_at;
        public DateTime? updated_at;
    }
}
