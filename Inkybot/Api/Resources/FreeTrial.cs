using System;

namespace Inkybot.Api.Resources
{
    #pragma warning disable 8618
    [Serializable]
    public class FreeTrial
    {
        public bool expired;
        public DateTime? created_at;
        public DateTime? updated_at;
    }
    #pragma warning restore 8618
}
