using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMusicBot.ServiceFolder.LoggerFolder;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscordMusicBot
{
    public class CommandHandler
    {
        private Logger Logger { get; }
        private DiscordSocketClient Client { get; }
        private CommandService CmdService { get; }
        private IServiceProvider ServiceProvider { get; set; }

        public CommandHandler(DiscordSocketClient client,
            CommandService cmdService)
        {
            Logger = new Logger();

            Client = client;
            CmdService = cmdService;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            this.ServiceProvider = provider;
            await CmdService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            CmdService.Log += LogAsync;
            Client.MessageReceived += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage socketMessage)
        {
            var argPos = 0;
            if (socketMessage.Author.IsBot) return;

            var userMessage = socketMessage as SocketUserMessage;
            if (userMessage is null)
                return;

            if (!userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(Client, userMessage);
            var result = await CmdService.ExecuteAsync(context, argPos, ServiceProvider);
        }

        private Task LogAsync(LogMessage msg)
        {
            Logger.Log(LogLevel.Information, new EventId(), msg.Exception, msg.Message);
            return Task.CompletedTask;
        }
    }
}