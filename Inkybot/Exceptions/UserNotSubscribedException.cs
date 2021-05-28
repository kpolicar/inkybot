namespace Inkybot.Exceptions
{
    public class UserNotSubscribedException : UserAuthException
    {
        public UserNotSubscribedException() : base("User is not subscribed!") {
        }
    }
}
