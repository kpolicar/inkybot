using System;
using Inkybot.Resources.Api;

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
