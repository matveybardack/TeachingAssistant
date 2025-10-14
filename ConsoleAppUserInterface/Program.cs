using System;
using System;
using System.IO;
using System.Linq;
using ClassLibraryTicketGenerator.Services;
using ClassLibraryTicketGenerator.Models;


namespace ConsoleAppUserInterface
{
    internal class Program
    {
        // Пить к данным
        private const string TasksFilePath = "tasks.txt";
        private const string TicketsFilePath = "tickets.txt";

        static void Main(string[] args)
        {
            var taskReader = new TaskReader();
            // Чтение метаданных из входного файла (имеется ввиду пользовательские мета)
            var allTasks = taskReader.ReadTasks(TasksFilePath).ToList();

            if (!allTasks.Any())
            {
                Console.WriteLine($"Ошибка: Файл с заданиями '{TasksFilePath}' пуст или не может быть прочитан.");
                return;
            }

            var ticketWriter = new TicketWriter(TicketsFilePath, TasksFilePath);
            var ticketGenerator = new TicketGenerator(ticketWriter);

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n--- Меню ---");
                Console.WriteLine("1. Генерация билетов");
                Console.WriteLine("2. Просмотр сгенерированных билетов");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите действие: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        GenerateTickets(ticketGenerator, allTasks);
                        break;
                    case "2":
                        ViewGeneratedTickets();
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод.");
                        break;
                }
            }
        }

        private static void GenerateTickets(TicketGenerator ticketGenerator, List<ClassLibraryTicketGenerator.Models.Task> allTasks)
        {
            Console.WriteLine("\n--- Генерация новых билетов ---");
            try
            {
                Console.Write("Введите сложность билета: ");
                if (!int.TryParse(Console.ReadLine(), out int targetComplexity))
                {
                    Console.WriteLine("Неправильный формат сложности.");
                    return;
                }

                Console.Write("Введите погрешность сложности билетов (%): ");
                if (!int.TryParse(Console.ReadLine(), out int tolerance))
                {
                    Console.WriteLine("Неверный формат погрешности.");
                    return;
                }

                Console.WriteLine("\n - Начало генерации. - ");
                ticketGenerator.Generate(allTasks, targetComplexity, tolerance);
                Console.WriteLine("\n - Генерация завершена. -");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static void ViewGeneratedTickets()
        {
            Console.WriteLine("\n--- Просмотр сгенерированных билетов ---");
            if (!File.Exists(TicketsFilePath))
            {
                Console.WriteLine("Не найден файл с билетами.");
                return;
            }

            try
            {
                string content = File.ReadAllText(TicketsFilePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("Файл с билетами пуст.");
                }
                else
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения файла с билетами: {ex.Message}");
            }
        }
    }
}
