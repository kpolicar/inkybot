namespace Inkybot.Exceptions
{
    public class GameLanguageChangedException : OcrException
    {
        public GameLanguageChangedException(string message)
            : base(message)
        {
        }
    }
}
