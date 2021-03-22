namespace Inkybot.Domain
{
    public class Server
    {
        public static readonly string ConfigsUrl = "https://forum.cheat-gam3.com/forums/configs.1712/";
        public static readonly string HallOfFameUrl = "https://forum.cheat-gam3.com/forums/hall-of-fame.1714/";
        public static readonly string BaseUrl = Program.Url;
        public static readonly string ApiUrl = $"{BaseUrl}/api/{Program.VersionEndpoint}";
        public static readonly string AuthUrl = $"{BaseUrl}/oauth-v2";
    }
}
