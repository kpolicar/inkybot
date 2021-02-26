using System;

namespace Inkybot.Exceptions
{
    public class DofusProcessDetachedException : SystemException
    {
        public DofusProcessDetachedException(string message) : base(message) {
        }
    }
}
