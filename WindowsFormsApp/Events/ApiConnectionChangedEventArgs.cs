using System;
using WindowsFormsApp.Api;

namespace WindowsFormsApp.Events
{
    public class ApiConnectionChangedEventArgs : EventArgs
    {
        public readonly ApiConnection connection;

        public ApiConnectionChangedEventArgs(ApiConnection connection) {
            this.connection = connection;
        }
    }
}