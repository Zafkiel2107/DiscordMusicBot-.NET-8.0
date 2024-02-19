namespace DiscordMusicBot.ConfigFolder
{
    public class Config
    {
        public Config(string token,
            string autorization,
            string hostname,
            ushort port)
        {
            Token = token;
            Autorization = autorization;
            Hostname = hostname;
            Port = port;
        }
        public string Token {  get; set; }
        public string Autorization { get; set; }
        public string Hostname { get; set; }
        public ushort Port { get; set; }
    }
}
