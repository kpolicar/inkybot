using System;
using Inkybot.Events;

namespace Inkybot
{
    public class ActionHandler
    {
        public event EventHandler<ActionExecutedEventArgs> ActionExecuted;

        public void Execute(IAction action) {
            action.Execute();
            ActionExecuted?.Invoke(this, new ActionExecutedEventArgs(action));
        }
    }
}
