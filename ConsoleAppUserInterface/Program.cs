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
        // Configuration
        private const string TasksFilePath = "tasks.txt";
        private const string TicketsFilePath = "tickets.txt";

        static void Main(string[] args)
        {
            var taskReader = new TaskReader();
            // We read all task metadata into memory once.
            var allTasks = taskReader.ReadTasks(TasksFilePath).ToList();
            if (!allTasks.Any())
            {
                Console.WriteLine($"Error: The task file '{TasksFilePath}' is empty or could not be read.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            var ticketWriter = new TicketWriter(TicketsFilePath, TasksFilePath);
            var ticketGenerator = new TicketGenerator(ticketWriter);

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n--- Ticket Generator Menu ---");
                Console.WriteLine("1. Generate Tickets");
                Console.WriteLine("2. View Generated Tickets");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

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
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static void GenerateTickets(TicketGenerator ticketGenerator, List<ClassLibraryTicketGenerator.Models.Task> allTasks)
        {
            Console.WriteLine("\n--- Generate New Tickets ---");
            try
            {
                Console.Write("Enter target complexity: ");
                if (!int.TryParse(Console.ReadLine(), out int targetComplexity))
                {
                    Console.WriteLine("Invalid number for complexity.");
                    return;
                }

                Console.Write("Enter complexity tolerance (%): ");
                if (!int.TryParse(Console.ReadLine(), out int tolerance))
                {
                    Console.WriteLine("Invalid number for tolerance.");
                    return;
                }

                Console.WriteLine("\nStarting generation process...");
                ticketGenerator.Generate(allTasks, targetComplexity, tolerance);
                Console.WriteLine("Generation process finished.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ViewGeneratedTickets()
        {
            Console.WriteLine("\n--- Viewing Generated Tickets ---");
            if (!File.Exists(TicketsFilePath))
            {
                Console.WriteLine("No tickets have been generated yet. Please use option 1 first.");
                return;
            }

            try
            {
                string content = File.ReadAllText(TicketsFilePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("The ticket file is empty.");
                }
                else
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the ticket file: {ex.Message}");
            }
        }
    }
}
