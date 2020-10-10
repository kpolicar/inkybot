using System;
using WindowsFormsApp.Resources.Api;

namespace WindowsFormsApp.Events
{
    public class FetchedUserEventArgs : EventArgs
    {
        public readonly User user;

        public FetchedUserEventArgs(User user) {
            this.user = user;
        }
    }
}
