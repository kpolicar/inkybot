using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Domain;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private struct ItemInfo
        {
            public Dictionary<Stat, UserRune[]> Runes;
        }
    }
}
