using System;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Events;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public class ActionHandler : HasDependencies
    {
        public event EventHandler<ActionExecutedEventArgs>? ActionExecuted;
        private CancellationTokenSource? cancelExecutingTask;
        private DofusMagingJob magingJob = null!;

        
        public void BindDependencies(ServiceContainer serviceContainer) {
            magingJob = serviceContainer.GetService<DofusMagingJob>();
            magingJob.Stopped += OnStoppedMaging;
        }

        private void OnStoppedMaging(object sender, EventArgs e) {
            cancelExecutingTask?.Cancel();
        }

        public void Execute(IAction action) {
            if (!magingJob.IsMaging)
                return;
            
            if (action is InputAction inputAction) {
                cancelExecutingTask = new CancellationTokenSource();
                var cancel = cancelExecutingTask.Token;

                try {
                    inputAction.Execute(cancel);
                    ActionExecuted?.Invoke(this, new ActionExecutedEventArgs(action));
                } catch (OperationCanceledException) {
                }
                
            } else {
                action.Execute();
            }
        }
    }
}
