using ClassLibraryTicketGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryTicketGenerator.Services
{
    public class TicketGenerator
    {
        private readonly TicketWriter _ticketWriter;

        public TicketGenerator(TicketWriter ticketWriter)
        {
            _ticketWriter = ticketWriter;
        }

        /// <summary>
        /// Главный метод генерации (сервис).
        /// </summary>
        public void Generate(List<Models.Task> allTasks, int targetComplexity, int tolerance)
        {
            _ticketWriter.Initialize();

            var typeRatio = CalculateTypeRatio(allTasks);
            if (!typeRatio.Any())
            {
                Console.WriteLine("Генерация остановлена. Недостаточно заданий для генерации.");
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
                    Console.WriteLine($"Сгенерирован Билет #{newTicket.TicketNumber}.");
                }
                else
                {
                    Console.WriteLine("Генерация остановлена. Не найдена уникальная комбинация заданий для билета.");
                    break;
                }
            }
        }

        private List<int> FindCombination(List<Models.Task> availableTasks, Dictionary<string, int> typeRatio, Dictionary<string, List<Models.Task>> tasksByType, double targetComplexity, int tolerance, int tasksPerTicket, HashSet<HashSet<int>> existingTickets, Random random)
        {
            var shuffledTasks = availableTasks.OrderBy(x => random.Next()).ToList();
            var combinations = GetCombinations(shuffledTasks, tasksPerTicket); // сюда возвращается ошибка

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
            // ✅ Убрано жёсткое сравнение с typeRatio, т.к. теперь приоритет у сложности
            // (typeRatio не используется в MVP, по спецификации)
            // Поэтому просто проверим, что все задачи уникальны
            if (tasks.Select(t => t.Id).Distinct().Count() != tasks.Count)
                return false;

            // 🔁 Было: проверка по темам, оставим, если нужно разнообразие
            if (tasks.Select(t => t.Theme).Distinct().Count() < 2)
                return false;

            // ✅ Изменено: теперь считаем не среднее, а СУММУ сложностей
            double totalComplexity = tasks.Sum(t => t.Complexity);

            // ✅ Изменено: теперь диапазон по сумме сложностей, а не по средней
            double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            double maxComplexity = targetComplexity * (1 + tolerance / 100.0);

            // Проверка попадания в диапазон
            if (totalComplexity < minComplexity || totalComplexity > maxComplexity)
                return false;

            // Проверка уникальности билета
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

    // Хранение хэшсетов из других хэшсетов
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
            foreach (T item in obj.OrderBy(i => i)) // Порядок обеспечения согласованности хэш-кода
            {
                hashCode ^= item.GetHashCode();
            }
            return hashCode;
        }
    }
}

