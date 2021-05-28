using System;

namespace Inkybot.Exceptions
{
    public class OperationRequiresUserConfirmationException : ApplicationException
    {
        public OperationRequiresUserConfirmationException() : base("User confirmation required!") {
        }
    }
}
