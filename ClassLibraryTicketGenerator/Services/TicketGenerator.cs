using ClassLibraryTicketGenerator.Models;

namespace ClassLibraryTicketGenerator.Services
{
    public class TicketGenerator
    {
        private readonly TicketWriter _ticketWriter;
        private const int MaxAttemptsPerTicket = 5_000;
        private readonly List<Models.Task> _tasks; // задания

        public List<Models.Task> Tasks { get { return _tasks; } }

        public TicketGenerator(TicketWriter ticketWriter, List<Models.Task> tasks)
        {
            _ticketWriter = ticketWriter;
            _tasks = tasks;
        }

        /// <summary>
        /// Главный метод генерации (сервис).
        /// </summary>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> относительная погрешеность сложности </param>
        public void Generate(int targetComplexity, int tolerance)
        {
            // Обновление выходного файла
            _ticketWriter.Initialize();

            var (selectedTypes, estimatedTaskCount, minTasks, maxTasks) = AnalyzeTypes( targetComplexity, tolerance);

            // Генерация всех комбинаций типов
            var typeCombinations = new List<List<string>>();
            for (int n = minTasks; n <= maxTasks; n++)
                typeCombinations.AddRange(GetTypeCombinations(selectedTypes, n));

            if (!typeCombinations.Any())
            {
                Console.WriteLine("Не удалось сгенерировать комбинации типов.");
                return;
            }

            // Выбор оптимальной комбинации
            var (bestCombination, bestTickets) = SelectBestCombination(typeCombinations, targetComplexity, tolerance);

            // Запись билетов в файл
            int ticketCounter = 1;
            foreach (var ticketIds in bestTickets)
                _ticketWriter.AppendTicket(new Ticket(ticketCounter++, ticketIds.ToList()));

            Console.WriteLine($"Сгенерировано {bestTickets.Count} билетов для комбинации типов: {string.Join(", ", bestCombination)}");
        }

        /// <summary>
        /// Выбор лучшей комбинации типов заданий по количеству сгенерированных билетов
        /// </summary>
        /// <param name="typeCombinations"> все комбинации типов </param>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> процентная погрешность сложности </param>
        /// <returns> кортеж из лучшей комбинации типов и списка хэшов заданий сгенерированных билетов </returns>
        private (List<string> bestCombo, List<HashSet<int>> bestTickets) SelectBestCombination(
            List<List<string>> typeCombinations,
            int targetComplexity,
            int tolerance)
        {
            List<HashSet<int>> bestTickets = null;
            List<string> bestCombo = null;

            // рандомайзер
            var random = new Random();

            foreach (var combo in typeCombinations)
            {
                var tickets = GenerateTicketsForCombination(combo, targetComplexity, tolerance, random);
                if (bestTickets == null || tickets.Count > bestTickets.Count)
                {
                    bestTickets = tickets;
                    bestCombo = combo;
                }
            }

            return (bestCombo, bestTickets ?? new List<HashSet<int>>());
        }

        /// <summary>
        /// Генерация всех билетов для одной комбинации типов заданий
        /// </summary>
        /// <param name="combo"> список комбинаций </param>
        /// <param name="targetComplexity"> заданная сложнгсть  </param>
        /// <param name="tolerance"> относительная погрешность сложности </param>
        /// <returns> Список хэшсетов с ID заданий в билетах </returns>
        private List<HashSet<int>> GenerateTicketsForCombination(
            List<string> combo,
            int targetComplexity,
            int tolerance,
            Random random)
        {
            var tickets = new List<HashSet<int>>();

            // Группировка по типу
            var tasksByType = Tasks.GroupBy(t => t.Type)
                                      .ToDictionary(g => g.Key, g => g.ToList());

            int attempts = MaxAttemptsPerTicket;
            while (attempts > 0)
            {
                var ticketTasks = new List<Models.Task>();
                bool failed = false;

                foreach (var type in combo)
                {
                    if (tasksByType[type].Count == 0)
                    {
                        failed = true;
                        break;
                    }

                    var availableTasks = tasksByType[type].Except(ticketTasks).ToList();
                    if (!availableTasks.Any())
                    {
                        failed = true;
                        break;
                    }

                    ticketTasks.Add(availableTasks[random.Next(availableTasks.Count)]);
                }

                if (!failed &&
                    ticketTasks.Sum(t => t.Complexity) is int total &&
                    total >= targetComplexity * (1 - tolerance / 100.0) &&
                    total <= targetComplexity * (1 + tolerance / 100.0) &&
                    ticketTasks.Select(t => t.Theme).Distinct().Count() > 1)
                {
                    var ticketIds = ticketTasks.Select(t => t.Id).ToHashSet();
                    if (!tickets.Any(t => t.SetEquals(ticketIds)))
                        tickets.Add(ticketIds);
                }

                attempts--;
            }

            return tickets;
        }

        /// <summary>
        /// Анализ типов заданий для выбора подходящих по средней сложности
        /// </summary>
        /// <param name="targetComplexity"> заданная сложность </param>
        /// <param name="tolerance"> процентная погрешность сложности </param>
        /// <returns> подходящие типы заданий, оценочное количество заданий в билете, минимальное число заданий, максимальное число заданий</returns>
        private (List<string> SelectedTypes, int EstimatedTaskCount, int MinTasks, int MaxTasks)
            AnalyzeTypes(int targetComplexity, int tolerance)
        {
            var typeStats = Tasks
                .GroupBy(t => t.Type)
                .ToDictionary(
                    g => g.Key,
                    g => (Count: g.Count(), AvgComplexity: g.Average(t => t.Complexity))
                );

            double avgComplexityForAll = Tasks.Average(t => t.Complexity);

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
            // Недостижимое условие (на всякий случай)
            if (length <= 0) yield break;

            var n = types.Count;

            // индексы комбинации (неубывающая последовательность индексов)
            var indices = new int[length];

            while (true)
            {
                var combo = new List<string>();

                for (int i = 0; i < length; i++)
                    combo.Add(types[indices[i]]);

                yield return combo;

                // инкрементируем "комбинацию" как счётчик с ограничением неубывания
                int pos = length - 1;
                while (pos >= 0 && indices[pos] == n - 1) pos--;
                if (pos < 0) yield break; // закончили — все индексы равны n-1

                // увеличение текущкей позиции и сохранение неубывания
                indices[pos]++;
                for (int j = pos + 1; j < length; j++)
                    indices[j] = indices[pos];
            }
        }

    }
}

