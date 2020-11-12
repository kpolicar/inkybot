using System;
using Inkybot.Domain;
using Inkybot.Events;

namespace Inkybot.Services
{
    public class ActionHandler
    {
        public event EventHandler<ActionExecutedEventArgs> ActionExecuted;

        public void Execute(IAction action) {
            var magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            if (!magingJob.IsMaging)
                return;
            action.Execute();
            ActionExecuted?.Invoke(this, new ActionExecutedEventArgs(action));
        }
    }
}
