using System;

namespace Inkybot.Events
{
    public class ActionExecutedEventArgs : EventArgs
    {
        public readonly IAction action;

        public ActionExecutedEventArgs(IAction action) {
            this.action = action;
        }
    }
}
