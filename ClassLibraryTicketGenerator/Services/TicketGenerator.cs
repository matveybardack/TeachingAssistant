using ClassLibraryTicketGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryTicketGenerator.Services
{
    public class TicketGenerator
    {
        private readonly TicketWriter _ticketWriter;
        private const int MaxAttemptsPerTicket = 5000;

        public TicketGenerator(TicketWriter ticketWriter)
        {
            _ticketWriter = ticketWriter;
        }

        /// <summary>
        /// Главный метод генерации (сервис).
        /// </summary>
        /// <param name="allTasks"> список всех заданий </param>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> относительная погрешеность сложности </param>
        public void Generate(List<Models.Task> allTasks, int targetComplexity, int tolerance)
        {
            const int MaxAttempts = 1000; // Ограничение на 1000 попыток собрать билет, чтобы избежать долгого исполнения (можно изменить)

            _ticketWriter.Initialize();

            var generatedTickets = new HashSet<HashSet<int>>();
            var random = new Random();
            int ticketCounter = 1;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var newTicketTasks = FindCombination(allTasks, targetComplexity, tolerance, generatedTickets, random);

                if (newTicketTasks != null && newTicketTasks.Count > 0)
                {
                    var newTicket = new Ticket(ticketCounter++, newTicketTasks.Select(t => t.Id).ToList());
                    var newTicketIdSet = new HashSet<int>(newTicket.TaskIds);

                    if (generatedTickets.Add(newTicketIdSet))
                    {
                        _ticketWriter.AppendTicket(newTicket);
                        attempt = 0; // Сброс счетчика попыток 
                    }
                }
            }
            
            Console.WriteLine($"Генерация завершена. Всего создано билетов: {generatedTickets.Count}.");

            if (!generatedTickets.Any())
            {
                Console.WriteLine("Не удалось сгенерировать ни одного билета по заданным критериям.");
            }
        }

        /// <summary>
        /// Поиск комбинации задач для одного билета по условиям
        /// </summary>
        /// <param name="allTasks"> список метаданных всех задачи </param>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> погрешность сложности в % </param>
        /// <param name="existingTickets"> множество сгенерированных билетов </param>
        /// <param name="random"> рандомайзер для генерации </param>
        /// <returns> Комбинацию, если валидна, иначе null </returns>
        private List<Models.Task> FindCombination(List<Models.Task> allTasks, double targetComplexity, int tolerance, HashSet<HashSet<int>> existingTickets, Random random)
        {
            var shuffledTasks = allTasks.OrderBy(t => random.Next()).ToList(); // Перемешивание списка задач
            var currentCombination = new List<Models.Task>();
            double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            double maxComplexity = targetComplexity * (1 + tolerance / 100.0);

            // Добавление случайных задач, пока не превысим макс. сложность
            foreach (var task in shuffledTasks)
            {
                if (currentCombination.Sum(t => t.Complexity) + task.Complexity <= maxComplexity)
                {
                    currentCombination.Add(task);
                }
            }

            // Проверка попадания составленного билета на валидность
            if (IsValidCombination(currentCombination, targetComplexity, tolerance, existingTickets))
            {
                return currentCombination;
            }

            return null; // Если комбинация не подошла
        }

        /// <summary>
        /// Проверка соответствия билета всем условиям
        /// </summary>
        /// <param name="tasks"> список заданий в билете </param>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> процентная погрешность сложности </param>
        /// <param name="existingTickets"> множество, существующих билетов </param>
        /// <returns></returns>
        private bool IsValidCombination(List<Models.Task> tasks, double targetComplexity, int tolerance, HashSet<HashSet<int>> existingTickets)
        {
            if (tasks == null || !tasks.Any()) return false;

            // Проверка сложности (изменил сам алгоритм)
            //double totalComplexity = tasks.Sum(t => t.Complexity);
            //double minComplexity = targetComplexity * (1 - tolerance / 100.0);
            //double maxComplexity = targetComplexity * (1 + tolerance / 100.0);
            //if (totalComplexity < minComplexity || totalComplexity > maxComplexity)
            //{
            //    return false;
            //}

            // Проверка на разнообразие типов
            if (tasks.Select(t => t.Type).Distinct().Count() < 2 && tasks.Count > 1)
            {
                return false;
            }

            // Проверка на разнообразие тем
            if (tasks.Select(t => t.Theme).Distinct().Count() < 2 && tasks.Count > 1)
            {
                return false;
            }

            // Уникальность задач в билете (изменил сам алгоритм)
            //if (tasks.Select(t => t.Id).Distinct().Count() != tasks.Count)
            //{
            //    return false;
            //}

            // Уникальность самого билета
            var currentTicketSignature = new HashSet<int>(tasks.Select(t => t.Id));
            if (existingTickets.Any(existing => existing.SetEquals(currentTicketSignature)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Анализ типов заданий для выбора подходящих по средней сложности
        /// </summary>
        /// <param name="allTasks"> список всех заданий </param>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> процентная погрешность сложности </param>
        /// <returns> подходящие типы заданий, оценочное количество заданий в билете, минимальное число заданий, максимальное число заданий</returns>
        private (List<string> SelectedTypes, int EstimatedTaskCount, int MinTasks, int MaxTasks)
            AnalyzeTypes(List<Models.Task> allTasks, int targetComplexity, int tolerance)
        {
            var typeStats = allTasks
                .GroupBy(t => t.Type)
                .ToDictionary(
                    g => g.Key,
                    g => (Count: g.Count(), AvgComplexity: g.Average(t => t.Complexity))
                );

            double avgComplexityForAll = allTasks.Average(t => t.Complexity);

            double lowerBound = avgComplexityForAll * (1 - tolerance / 100.0);
            double upperBound = avgComplexityForAll * (1 + tolerance / 100.0);

            var suitableTypes = typeStats
                .Where(kvp => kvp.Value.AvgComplexity >= lowerBound && kvp.Value.AvgComplexity <= upperBound)
                .OrderByDescending(kvp => kvp.Value.Count)
                .ToList();

            if (!suitableTypes.Any())
            {
                Console.WriteLine("Предупреждение: не найдено типов в диапазоне сложности, будут использованы все типы.");
                suitableTypes = typeStats.OrderByDescending(kvp => kvp.Value.Count).ToList();
            }

            int minTasks = Math.Max(2, (int)Math.Round(targetComplexity * (1 - tolerance / 100.0) / avgComplexityForAll));
            int maxTasks = Math.Max(minTasks, (int)Math.Round(targetComplexity * (1 + tolerance / 100.0) / avgComplexityForAll));

            int estimatedTaskCount = Enumerable.Range(minTasks, maxTasks - minTasks + 1)
                .OrderBy(n => Math.Abs(targetComplexity - avgComplexityForAll * n))
                .First();

            var selectedTypes = suitableTypes.Select(kvp => kvp.Key).ToList();

            return (selectedTypes, estimatedTaskCount, minTasks, maxTasks);
        }

        /// <summary>
        /// Генерирует все комбинации типов заданий длиной <paramref name="length"/>.
        /// Комбинации допускают повторение типов, порядок не имеет значения.
        /// </summary>
        /// <param name="types">Список доступных типов заданий.</param>
        /// <param name="length">Длина каждой комбинации.</param>
        /// <returns>Последовательность списков типов (каждый список — одна комбинация).</returns>
        private static IEnumerable<List<string>> GetTypeCombinations(List<string> types, int length)
        {
            // Базовый случай: длина комбинации == 1
            if (length == 1)
                return types.Select(t => new List<string> { t });

            // Рекурсивная генерация комбинаций длиной length-1
            var shorterCombos = GetTypeCombinations(types, length - 1);

            return shorterCombos.SelectMany(
                prevCombo =>
                    // Добавляем только те типы, которые >= последнего в prevCombo,
                    // чтобы исключить дублирующие перестановки
                    types.Where(t => string.Compare(t, prevCombo.Last(), StringComparison.Ordinal) >= 0),
                (prevCombo, nextType) =>
                {
                    var newCombo = new List<string>(prevCombo) { nextType };
                    return newCombo;
                });
        }

    }
}

