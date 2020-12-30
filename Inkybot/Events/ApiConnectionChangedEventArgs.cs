using System;
using Inkybot.Api;

namespace Inkybot.Events
{
    public class ApiConnectionChangedEventArgs : EventArgs
    {
        public readonly ApiConnection? connection;

        public ApiConnectionChangedEventArgs(ApiConnection? connection) {
            this.connection = connection;
        }
    }
}
