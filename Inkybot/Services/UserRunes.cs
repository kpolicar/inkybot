using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    public class UserRunes : Dictionary<Stat, UserRune[]>
    {
        public UserRune Find(Rune rune) =>
            this.SelectMany(userRunes => userRunes.Value)
                .First(userRune => userRune.Rune.Equals(rune));
    }
}
