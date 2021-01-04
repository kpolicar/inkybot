using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Domain;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        public struct CurrentItemInfo
        {
            public Dictionary<Stat, UserRune[]> Runes;

        }
    }
}
