using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;
using System.Threading;

namespace TelegramBot
{
    class Program
    {

        static async Task Main(string[] args)
        {
            string apiBot = "7559223550:AAFOA1eqLqRY3EPoxR68SjWhp15eZzEihNw";
            var botClient = new TelegramBotClient(apiBot);

            Console.WriteLine($"Запущен bot @{(await botClient.GetMe()).Username}");


            using var cts = new CancellationTokenSource();
            var timer = new System.Threading.Timer(
                        async _ => await SendBirthdayNotifications(cts.Token, botClient),
                        null,
                        TimeSpan.Zero,                 
                        TimeSpan.FromDays(1)           
                            );

            botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: cts.Token
        );

            Console.WriteLine("Нажмите любую клавишу для остановки бота...");
            Console.ReadKey();

            cts.Cancel();

        }
        static async Task SendBirthdayNotifications(CancellationToken cancellation, TelegramBotClient botClient) 
        {
            var nowDate = DateTime.Now.ToString("MM-dd");
            var birthdayService = new BirthdayService("birthdays.json");
            var birthdays = birthdayService.GetBirthdays();
            foreach (var birthday in birthdays) 
            {
                if (birthday.Date.Substring(5) == nowDate) 
                {
                   
                    string mention = $"🎉 Сегодня день рождения у <a>{birthday.UserName}</a>! Поздравляем! 🎂";

                   
                    await botClient.SendTextMessageAsync(
                        chatId: birthday.ChatId,
                        text: mention,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        cancellationToken: cancellation
                    );
                }
            }


        }
        static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message) return;
            if (message.Text is not { } messageText) return;

            Console.WriteLine($"Получено сообщение: '{messageText}' от @{message.Chat.Username}");

            string response = messageText.Split(' ')[0] switch
            {
                "/start" => "Привет! Я бот для напоминания о днях рождения."
              /*  "/add" => AddBirthday(messageText, message.Chat.Id),
                "/list" => ListBirthdays(),
                _ => "Неизвестная команда. Используйте /start для справки."*/
            };
            

            await bot.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: cancellationToken);
        }

        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiException => $"Ошибка API Telegram: {apiException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        static string AddBirthday(string commandText, long chatId)
        {
            var birthdayService = new BirthdayService("birthdays.json");
            try
            {
                var parts = commandText.Split(' ', 3);
                if (parts.Length < 3) return "Неверный формат. Используйте: /add Имя YYYY-MM-DD";

                string name = parts[1];
                string date = parts[2];

                if (!DateTime.TryParse(date, out _))
                    return "Неверный формат даты. Используйте: YYYY-MM-DD";

                birthdayService.AddBirthday(name, date, chatId);
                return $"День рождения {name} ({date}) добавлен!";
            }
            catch
            {
                return "Ошибка при добавлении дня рождения.";
            }
        }

        static string ListBirthdays()
        {
            var birthdayService = new BirthdayService("birthdays.json");
            var birthdays = birthdayService.GetBirthdays();

            if (birthdays.Count == 0) return "Список дней рождения пуст.";

            return string.Join("\n", birthdays.Select(b => $"{b.Name}: {b.Date}"));
        }
    }
}
