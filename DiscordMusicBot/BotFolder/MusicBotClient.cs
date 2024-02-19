using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMusicBot.CommandsFolder;
using DiscordMusicBot.ConfigFolder;
using DiscordMusicBot.ServiceFolder.LoggerFolder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Victoria;
using Configuration = DiscordMusicBot.ServiceFolder.Configuration;


namespace DiscordMusicBot.BotFolder
{
    public class MusicBotClient
    {
        private Logger Logger { get; set; }
        private DiscordSocketClient Client { get; set; }
        private CommandService CmdService { get; }
        private IServiceProvider ServiceProvider { get; set; }
        private Config Config { get; }
        private Configuration ConfigurationService { get; }

        public MusicBotClient()
        {
            Logger = new Logger();

            DiscordSocketConfig config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
                    | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Debug
            };
            Client = new DiscordSocketClient(config);

            CmdService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false
            });

            ConfigurationService = new Configuration();
            Config = ConfigurationService.GetConfiguration();

            Client.Log += LogHandlerInformation;
        }

        public async Task InitializeAsync()
        {
            Logger.LogInformation(new EventId(), null, "Запуск discord бота DiscordMusicBot");

            await this.Client.LoginAsync(TokenType.Bot, Config.Token);
            await this.Client.StartAsync();
            ServiceProvider = SetupServices();

            var cmdHandler = new CommandHandler(Client, CmdService);
            await cmdHandler.InitializeAsync(ServiceProvider);

            await ServiceProvider.GetRequiredService<MusicCommands>().InitializeAsync();

            await Task.Delay(-1);
        }
        private Task LogHandlerInformation(LogMessage msg)
        {
            Logger.Log(LogLevel.Information, new EventId(), msg.Exception, msg.Message);
            return Task.CompletedTask;
        }

        private IServiceProvider SetupServices()
            => new ServiceCollection()
            .AddSingleton(Client)
            .AddSingleton(Logger)
            .AddLavaNode(x =>
            {
                x.Hostname = Config.Hostname;
                x.Port = Config.Port;
                x.Authorization = Config.Autorization;
            })
            .AddSingleton<MusicCommands>()
            .BuildServiceProvider();
    }
}