using System;
using Inkybot.Contracts;

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
