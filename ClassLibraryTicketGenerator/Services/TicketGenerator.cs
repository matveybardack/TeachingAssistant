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

            var generatedTickets = new HashSet<HashSet<int>>(new HashSetEqualityComparer<int>());
            var random = new Random();
            int ticketCounter = 1;
            const int maxAttempts = 1000; // Ограничение, чтобы избежать бесконечного цикла

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var newTicketTasks = FindCombination(allTasks, targetComplexity, tolerance, generatedTickets, random);

                if (newTicketTasks != null && newTicketTasks.Count > 0)
                {
                    var newTicket = new Ticket(ticketCounter++, newTicketTasks.Select(t => t.Id).ToList());
                    var newTicketIdSet = new HashSet<int>(newTicket.TaskIds);

                    // Проверяем уникальность билета еще раз, на случай гонки состояний
                    if (generatedTickets.Add(newTicketIdSet))
                    {
                        _ticketWriter.AppendTicket(newTicket);
                        Console.WriteLine($"Сгенерирован Билет #{newTicket.TicketNumber}.");
                        attempt = 0; // Сбрасываем счетчик попыток, так как нашли новый билет
                    }
                }
            }
            
            Console.WriteLine($"Генерация завершена. Всего создано билетов: {generatedTickets.Count}.");

            if (!generatedTickets.Any())
            {
                Console.WriteLine("Не удалось сгенерировать ни одного билета по заданным критериям.");
            }
        }

        private List<Models.Task> FindCombination(List<Models.Task> allTasks, double targetComplexity, int tolerance, HashSet<HashSet<int>> existingTickets, Random random)
        {
            var shuffledTasks = allTasks.OrderBy(t => random.Next()).ToList();
            var currentCombination = new List<Models.Task>();
            double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            double maxComplexity = targetComplexity * (1 + tolerance / 100.0);

            // Жадный алгоритм: добавляем случайные задачи, пока не превысим макс. сложность
            foreach (var task in shuffledTasks)
            {
                if (currentCombination.Sum(t => t.Complexity) + task.Complexity <= maxComplexity)
                {
                    currentCombination.Add(task);
                }
            }

            // Проверяем, попала ли итоговая комбинация в нужный диапазон и другие правила
            if (IsValidCombination(currentCombination, targetComplexity, tolerance, existingTickets))
            {
                return currentCombination;
            }

            return null; // Если комбинация не подошла
        }


        private bool IsValidCombination(List<Models.Task> tasks, double targetComplexity, int tolerance, HashSet<HashSet<int>> existingTickets)
        {
            if (tasks == null || !tasks.Any()) return false;

            // GR-A.1: Проверка на соответствие общей сложности
            double totalComplexity = tasks.Sum(t => t.Complexity);
            double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            double maxComplexity = targetComplexity * (1 + tolerance / 100.0);
            if (totalComplexity < minComplexity || totalComplexity > maxComplexity)
            {
                return false;
            }

            // GR-B.1: Проверка на разнообразие типов
            if (tasks.Select(t => t.Type).Distinct().Count() < 2 && tasks.Count > 1)
            {
                return false;
            }

            // GR-C.1: Проверка на разнообразие тем
            if (tasks.Select(t => t.Theme).Distinct().Count() < 2 && tasks.Count > 1)
            {
                return false;
            }
            
            // GR-D.1: Уникальность задач в билете
            if (tasks.Select(t => t.Id).Distinct().Count() != tasks.Count)
            {
                return false;
            }

            // GR-E.1: Уникальность самого билета
            var currentTicketSignature = new HashSet<int>(tasks.Select(t => t.Id));
            if (existingTickets.Any(existing => existing.SetEquals(currentTicketSignature)))
            {
                return false;
            }

            return true;
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

