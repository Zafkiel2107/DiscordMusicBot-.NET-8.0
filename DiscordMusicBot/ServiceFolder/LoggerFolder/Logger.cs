using Microsoft.Extensions.Logging;
using Victoria.Node;

namespace DiscordMusicBot.ServiceFolder.LoggerFolder
{
    public class Logger : ILogger<LavaNode>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string logPath = Path.GetFullPath("../../ServiceFolder/LoggerFolder/log.txt");
            if (!File.Exists(logPath))
            {
                FileStream f = File.Create(logPath);
                f.Close();
            }

            using (StreamWriter sw = File.AppendText(logPath))
            {
                File.AppendAllText(logPath, formatter(state, exception));
                Console.WriteLine(formatter(state, exception));
            }
        }
    }
}