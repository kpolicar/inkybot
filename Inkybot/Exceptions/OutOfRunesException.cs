using System;
using Inkybot.Domain;

namespace Inkybot.Exceptions
{
    public class OutOfRunesException : MagingException
    {
        public OutOfRunesException(Rune rune) : base($"You have ran out of: {rune}") {
        }
    }
}
