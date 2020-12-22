using System;
using Inkybot.Domain;

namespace Inkybot.Exceptions
{
    public class OutOfRunesException : MagingException
    {
        public readonly Rune Rune;

        public OutOfRunesException(Rune rune) : base($"You have run out of runes ({rune})!") {
            Rune = rune;
        }
    }
}
