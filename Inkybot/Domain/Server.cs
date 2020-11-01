using static System.Configuration.ConfigurationManager;

namespace Inkybot
{
    public class Server
    {
        public static readonly string BaseUrl = Program.Url;
        public static readonly string ApiUrl = $"{BaseUrl}/api/{Program.VersionEndpoint}";
        public static readonly string AuthUrl = $"{BaseUrl}/oauth";
    }
}
