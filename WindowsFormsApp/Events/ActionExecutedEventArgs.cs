using System;

namespace WindowsFormsApp.Events
{
    public class ActionExecutedEventArgs : EventArgs
    {
        public readonly IAction action;

        public ActionExecutedEventArgs(IAction action) {
            this.action = action;
        }
    }
}
