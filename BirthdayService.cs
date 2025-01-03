using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class BirthdayService
    {
        private readonly string filePath;
        private List<BirthdayEntry> birthdays;
        public BirthdayService(string filePath)
        {
            this.filePath = filePath;
            this.birthdays = LoadBirthdays();
        }

        public void AddBirthday(string name, string date, long chatId)
        {
            birthdays.Add(new BirthdayEntry { Name = name, Date = date, ChatId = chatId });
            SaveBirthdays();
        }

        public List<BirthdayEntry> GetBirthdays()
        {
            return birthdays;
        }

        private List<BirthdayEntry> LoadBirthdays()
        {
            if (!File.Exists(filePath))
                return new List<BirthdayEntry>();

            var json = File.ReadAllText("birthdays.json");
            Console.WriteLine(json);
            return JsonSerializer.Deserialize<List<BirthdayEntry>>(json) ?? new List<BirthdayEntry>();
        }

        private void SaveBirthdays()
        {
            var json = JsonSerializer.Serialize(birthdays, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, json);

        }

    }
    public class BirthdayEntry
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string UserName {  get; set; }
        public long ChatId { get; set; }

    }
}
