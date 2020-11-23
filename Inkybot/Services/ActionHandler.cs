using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inkybot.Domain;
using Inkybot.Events;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public class ActionHandler
    {
        public event EventHandler<ActionExecutedEventArgs> ActionExecuted;
        private CancellationTokenSource cancelExecutingTask = new CancellationTokenSource();
        private DofusMagingJob magingJob;

        public ActionHandler() {
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magingJob.Stopped += OnStoppedMaging;
        }

        private void OnStoppedMaging(object sender, EventArgs e) {
            cancelExecutingTask.Cancel();
        }

        public void Execute(IAction action) {
            if (!magingJob.IsMaging)
                return;
            
            var cancel = cancelExecutingTask.Token;
            var task = new Task(o => {
                action.Execute();
                ActionExecuted?.Invoke(this, new ActionExecutedEventArgs(action));
            }, cancel);
            
            task.Wait(cancel);
        }
    }
}
