using System;
using WindowsFormsApp.Events;

namespace WindowsFormsApp
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