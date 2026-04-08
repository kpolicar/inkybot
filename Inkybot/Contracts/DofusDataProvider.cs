using System;
using Inkybot.Dofus;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusDataProvider
    {
        public event EventHandler<ItemEventArgs>? FetchedItem;

        void FetchData();
        Item Item();
        string[] History();
    }
}
