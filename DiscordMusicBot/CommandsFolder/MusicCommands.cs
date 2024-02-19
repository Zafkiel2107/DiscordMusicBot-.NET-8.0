using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMusicBot.ServiceFolder.LoggerFolder;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

namespace DiscordMusicBot.CommandsFolder
{
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        private LavaNode LavaNode { get; }
        private DiscordSocketClient Client { get; }
        private Logger Logger { get; }

        public MusicCommands(LavaNode lavaNode,
            DiscordSocketClient client)
        {
            Logger = new Logger();

            Client = client;
            LavaNode = lavaNode;
        }

        public Task InitializeAsync()
        {
            Client.Ready += ClientReadyAsync;
            LavaNode.OnTrackEnd += TrackFinished;
            return Task.CompletedTask;
        }

        [Command("Join", RunMode = RunMode.Async)]
        public async Task Join()
        {
            await LavaNode.JoinAsync((Context.User as IGuildUser)?.VoiceChannel, Context.Channel as ITextChannel);
            await Context.Channel.SendMessageAsync($"Бот подключился к каналу {Context.Channel.Name}");
        }
        [Command("Leave")]
        public async Task Leave()
        {
            await LavaNode.LeaveAsync((Context.User as IGuildUser)?.VoiceChannel);
            await Context.Channel.SendMessageAsync($"Бот отключился от канала {Context.Channel.Name}");
        }
        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play([Remainder] string query)
        {
            if ((Context.User as IGuildUser)?.VoiceChannel == null)
                return;

            var isExist = LavaNode.TryGetPlayer(Context.Guild, out var player);

            if (player.PlayerState != PlayerState.None)
                await LavaNode.JoinAsync((Context.User as IGuildUser)?.VoiceChannel, Context.Channel as ITextChannel);

            if (!isExist)
            {
                await Context.Channel.SendMessageAsync("Плеер не найден.");
                return;
            }

            var search = await LavaNode.SearchAsync(SearchType.YouTube, query);
            if (search.Status != SearchStatus.LoadFailed ||
                search.Status != SearchStatus.NoMatches)
            {
                await Context.Channel.SendMessageAsync("Трек не найден.");
                return;
            }

            var track = search.Tracks.FirstOrDefault(x => x.Title == query);

            if (player.PlayerState is PlayerState.Playing)
            {
                player.Vueue.Enqueue(track);
                await Context.Channel.SendMessageAsync($"{track.Title} добавлен в очередь.");
            }
            else
            {
                await player.PlayAsync(track); //? вопрос о том, как отсеивать ненужное или пихать все треки целиком
                await Context.Channel.SendMessageAsync($"Cейчас играет: {track.Title}.");
            }
        }

        [Command(text: "Stop", RunMode = RunMode.Async)]
        public async Task Stop()
        {
            var isExist = LavaNode.TryGetPlayer(Context.Guild, out var player);
            if (!isExist)
            {
                await Context.Channel.SendMessageAsync("Плеер не найден.");
                return;
            }

            player.Vueue.Clear();
            await player.StopAsync();
            await Context.Channel.SendMessageAsync("Плеер бы остановлен.");
        }

        [Command("Pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            var isExist = LavaNode.TryGetPlayer(Context.Guild, out var player);
            if (!isExist)
            {
                await Context.Channel.SendMessageAsync("Плеер не найден.");
                return;
            }

            if (player.PlayerState is PlayerState.Playing)
            {
                await player.PauseAsync();
                await Context.Channel.SendMessageAsync("Плеер бы приостановлен.");
            }
            else
                return;
        }

        [Command("Resume", RunMode = RunMode.Async)]
        public async Task Resume()
        {
            var isExist = LavaNode.TryGetPlayer(Context.Guild, out var player);
            if (!isExist)
            {
                await Context.Channel.SendMessageAsync("Плеер не найден.");
                return;
            }

            if (player.PlayerState is PlayerState.Paused)
            {
                await player.ResumeAsync();
                await Context.Channel.SendMessageAsync("Плеер бы возобновлен.");
            }
            else
                return;
        }

        [Command("Skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            var isExist = LavaNode.TryGetPlayer(Context.Guild, out var player);
            if (!isExist || player.Vueue.Count() is 0)
            {
                await Context.Channel.SendMessageAsync("Плеер не найден или очередь на воспроизведение пуста.");
                return;
            };

            var oldTrack = player.Track;
            await player.SkipAsync();
            await Context.Channel.SendMessageAsync($"Пропущен трек: {oldTrack.Title}, сейчас играет: {player.Track.Title}.");
        }

        private async Task ClientReadyAsync()
        {
            if (!LavaNode.IsConnected)
            {
                await LavaNode.ConnectAsync();
            }
        }

        private async Task TrackFinished(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            if (arg.Reason is not (TrackEndReason.Finished or TrackEndReason.LoadFailed))
                return;

            if (!arg.Player.Vueue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await arg.Player.TextChannel.SendMessageAsync("Очередь на воспроизведение пуста.");
                await LavaNode.DisconnectAsync();
                return;
            }

            await arg.Player.PlayAsync(nextTrack);
        }
    }
}