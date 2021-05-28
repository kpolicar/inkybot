namespace Inkybot.Exceptions
{
    public class UserTrialHasExpiredException : UserAuthException
    {
        public UserTrialHasExpiredException() : base("Trial has expired") {
        }
    }
}
