using System;
using Inkybot.Api.Resources;

namespace Inkybot.Events
{
    public class FetchedUserEventArgs : EventArgs
    {
        public readonly User user;

        public FetchedUserEventArgs(User user) {
            this.user = user;
        }
    }
}
