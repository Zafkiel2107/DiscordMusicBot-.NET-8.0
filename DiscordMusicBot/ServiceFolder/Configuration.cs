using DiscordMusicBot.ConfigFolder;
using DiscordMusicBot.ServiceFolder.LoggerFolder;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscordMusicBot.ServiceFolder
{
    public class Configuration
    {
        private Logger Logger {  get; set; }
        public Configuration() 
        {
            Logger = new Logger();
        }
        public Config GetConfiguration()
        {
            var jsonPath = Path.GetFullPath("../../ConfigFolder/Config.json");
            Logger.LogInformation(new EventId(), null, "Запуск процедуры получения токена из json файла");
            try
            {
                using (FileStream fs = new FileStream(jsonPath, FileMode.Open))
                    return JsonSerializer.Deserialize<Config>(fs) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                Logger.LogError(new EventId(), ex, ex.Message);
                Logger.LogInformation(new EventId(), null, "Файл не найден или не указан токен, попытка использования дефолтных значений");
                return new Config("", "", "", 0); //установка дефолтных значений
            }
        }
    }
}