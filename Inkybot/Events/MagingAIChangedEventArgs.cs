using System;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;

namespace Inkybot.Events
{
    public class MagingAIChangedEventArgs : EventArgs
    {
        public readonly DofusMagingAI AI;

        public MagingAIChangedEventArgs(DofusMagingAI ai) {
            AI = ai;
        }
    }
}
