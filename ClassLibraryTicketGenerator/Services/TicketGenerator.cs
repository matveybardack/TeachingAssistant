using ClassLibraryTicketGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Service responsible for the core logic of generating tickets
    /// based on a set of rules.
    /// </summary>
    public class TicketGenerator
    {
        private readonly TicketWriter _ticketWriter;

        public TicketGenerator(TicketWriter ticketWriter)
        {
            _ticketWriter = ticketWriter;
        }

        /// <summary>
        /// Main method to orchestrate the ticket generation process.
        /// </summary>
        public void Generate(List<Models.Task> allTasks, int targetComplexity, int tolerance)
        {
            // Placeholder for the generation algorithm.
            // We will implement this in the next steps.
            System.Console.WriteLine("[DEBUG] Generation algorithm started.");

            _ticketWriter.Initialize();

            var typeRatio = CalculateTypeRatio(allTasks);
            if (!typeRatio.Any())
            {
                Console.WriteLine("Generation stopped. Reason: Not enough tasks to determine a type ratio.");
                return;
            }

            int tasksPerTicket = typeRatio.Values.Sum();
            var tasksByType = allTasks.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.ToList());
            var generatedTickets = new HashSet<HashSet<int>>(new HashSetEqualityComparer<int>());
            var random = new Random();
            int ticketCounter = 1;

            while (true)
            {
                var availableTasks = new List<Models.Task>(allTasks);
                var newTicketTaskIds = FindCombination(
                    availableTasks,
                    typeRatio,
                    tasksByType,
                    targetComplexity,
                    tolerance,
                    tasksPerTicket,
                    generatedTickets,
                    random
                );

                if (newTicketTaskIds != null)
                {
                    var newTicket = new Ticket(ticketCounter++, newTicketTaskIds);
                    _ticketWriter.AppendTicket(newTicket);
                    generatedTickets.Add(new HashSet<int>(newTicketTaskIds));
                    Console.WriteLine($"Successfully generated Ticket #{newTicket.TicketNumber}.");
                }
                else
                {
                    Console.WriteLine("Generation stopped. Could not find a valid combination for the next ticket.");
                    break;
                }
            }
            Console.WriteLine($"[DEBUG] Generation algorithm finished. Total tickets created: {ticketCounter - 1}.");
        }

        private List<int> FindCombination(List<Models.Task> availableTasks, Dictionary<string, int> typeRatio, Dictionary<string, List<Models.Task>> tasksByType, double targetComplexity, int tolerance, int tasksPerTicket, HashSet<HashSet<int>> existingTickets, Random random)
        {
            var shuffledTasks = availableTasks.OrderBy(x => random.Next()).ToList();
            var combinations = GetCombinations(shuffledTasks, tasksPerTicket);

            foreach (var combo in combinations)
            {
                var comboList = combo.ToList();
                if (IsValidCombination(comboList, typeRatio, targetComplexity, tolerance, existingTickets))
                {
                    return comboList.Select(t => t.Id).ToList();
                }
            }
            return null;
        }

        private bool IsValidCombination(List<Models.Task> tasks, Dictionary<string, int> typeRatio, double targetComplexity, int tolerance, HashSet<HashSet<int>> existingTickets)
        {
            var currentTypeCounts = tasks.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Count());
            if (typeRatio.Count != currentTypeCounts.Count || !typeRatio.All(kvp => currentTypeCounts.ContainsKey(kvp.Key) && currentTypeCounts[kvp.Key] == kvp.Value))
                return false;

            if (tasks.Select(t => t.Theme).Distinct().Count() < 2)
                return false;
            
            double avgComplexity = tasks.Average(t => t.Complexity);
            double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            double maxComplexity = targetComplexity * (1 + tolerance / 100.0);
            if (avgComplexity < minComplexity || avgComplexity > maxComplexity)
                return false;

            var currentTicketSignature = new HashSet<int>(tasks.Select(t => t.Id));
            if (existingTickets.Contains(currentTicketSignature))
                return false;

            return true;
        }

        private IEnumerable<IEnumerable<T>> GetCombinations<T>(List<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetCombinations(list, length - 1)
                .SelectMany(t => list.Where(e => Comparer<T>.Default.Compare(e, t.Last()) > 0),
                            (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private Dictionary<string, int> CalculateTypeRatio(List<Models.Task> allTasks)
        {
            if (allTasks == null || !allTasks.Any())
            {
                return new Dictionary<string, int>();
            }

            var typeCounts = allTasks
                .GroupBy(t => t.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            int gcd = 0;
            foreach (var count in typeCounts.Values)
            {
                gcd = Gcd(gcd, count);
            }

            if (gcd == 0) return new Dictionary<string, int>();

            return typeCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / gcd);
        }

        private int Gcd(int a, int b)
        {
            return b == 0 ? a : Gcd(b, a % b);
        }
    }

    // Custom comparer for HashSet<int> to allow storing them in another HashSet
    public class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
    {
        public bool Equals(HashSet<T> x, HashSet<T> y)
        {
            if (x == null || y == null)
                return x == y;

            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<T> obj)
        {
            if (obj == null)
                return 0;

            int hashCode = 0;
            foreach (T item in obj.OrderBy(i => i)) // Order to ensure hash code is consistent
            {
                hashCode ^= item.GetHashCode();
            }
            return hashCode;
        }
}
}

