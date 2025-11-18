using ClassLibraryTicketGenerator.Models;
using Task = ClassLibraryTicketGenerator.Models.Task;

namespace ClassLibraryTicketGenerator.Services
{
    public class TicketGenerator
    {
        private readonly TicketWriter _ticketWriter;
        private const int MaxAttemptsPerTicket = 10_000;
        private readonly TaskReader _taskReader; // задания

        private readonly Dictionary<int, int> _tasksPerType;
        private readonly int _targetDifficulty;
        private readonly int _tolerance;

        private readonly Random _random = new();

        // Список использованных TaskId
        private readonly HashSet<int> _usedTaskIds = new();

        public TicketGenerator(
            TicketWriter ticketWriter, 
            TaskReader taskReader,
            Dictionary<int, int> tasksPerType,
            int targetDifficulty,
            int tolerance)
        {
            _ticketWriter = ticketWriter;
            _taskReader = taskReader;
            _tasksPerType = tasksPerType;
            _targetDifficulty = targetDifficulty;
            _tolerance = tolerance;
        }


        /// <summary>
        /// Формирует словарь задач по типу и выполняет просеивание по допустимой сложности.
        /// Возвращает очищенный словарь: Type → List<Task>.
        /// </summary>
        public Dictionary<int, List<Task>> AnalyzeTypes()
        {
            var dict = LoadAndSortTasksByType();
            var filtered = FilterTasksByDifficulty(dict);
            return filtered;
        }

        /// <summary>
        /// Загрузка и сортировка заданий по типу.
        /// </summary>
        /// <returns> словарь, где Key - это индекс типа задачи, а Value - список задач одного типа </returns>
        private Dictionary<int, List<Task>> LoadAndSortTasksByType()
        {
            var dictionary = _taskReader.ReadTasks()
                .Where(t => _tasksPerType.ContainsKey(t.Type))
                .GroupBy(t => t.Type)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Сортировка внутри каждого типа
            foreach (var kvp in dictionary)
            {
                kvp.Value.Sort((a, b) => a.Complexity.CompareTo(b.Complexity));
            }

            return dictionary;
        }

        /// <summary>
        /// Фильтрация заданий внутри типов по допустимым интервалам сложности.
        /// </summary>
        /// <param name="dict"> нефильтрованный словарь </param>
        /// <returns> просеянный словарь задач </returns>
        /// <exception cref="ArgumentException"> какой-то тип не лежит в заданной сложности </exception>
        private Dictionary<int, List<Task>> FilterTasksByDifficulty(Dictionary<int, List<Task>> dict)
        {
            var result = new Dictionary<int, List<Task>>();

            // Наименьшая и наибольшая сложности в каждом типе
            var minComplexity = dict.ToDictionary(k => k.Key, v => v.Value.First().Complexity);
            var maxComplexity = dict.ToDictionary(k => k.Key, v => v.Value.Last().Complexity);

            foreach (var n in dict.Keys)
            {
                int N = _tasksPerType[n];  // количество задач этого типа

                // минимальная сумма сложностей всех остальных типов (минимумы)
                int sumOthersMin = _tasksPerType
                    .Where(kvp => kvp.Key != n)
                    .Sum(kvp => kvp.Value * minComplexity[kvp.Key]);

                // максимальная сумма сложностей остальных типов
                int sumOthersMax = _tasksPerType
                    .Where(kvp => kvp.Key != n)
                    .Sum(kvp => kvp.Value * maxComplexity[kvp.Key]);

                // Максимально допустимая сложность задачи типа n
                double Smax = (double)(_targetDifficulty + _tolerance - sumOthersMin) / N;

                // Минимально допустимая сложность задачи типа n
                double Smin = (double)(_targetDifficulty - _tolerance - sumOthersMax) / N;


                int left = dict[n].FindIndex(t => t.Complexity >= (int) Math.Floor(Smin));
                int right = dict[n].FindLastIndex(t => t.Complexity <= (int) Math.Ceiling(Smax));

                if (left == -1 || right == -1 || left > right)
                {
                    if (IdToString.TryConvertTypeIdToString(n, out string type))
                        throw new ArgumentException($"Тип {type} не подходит для составления билетов по данной сложности");
                }
                else
                {
                    var filteredList = dict[n].GetRange(left, right - left + 1);
                    result[n] = filteredList;
                }
            }

            return result;
        }

        /// <summary>
        /// Генерация билетов на основе отфильтрованных задач.
        /// </summary>
        /// <param name="filteredTasks"> просеянный словарь </param>
        /// <returns> список сгенерированный билетов </returns>
        private List<Ticket> Generate(Dictionary<int, List<Task>> filteredTasks)
        {
            var tasks = filteredTasks;
            var tickets = new List<Ticket>();
            int ticketNumber = 1;
            int tokens = MaxAttemptsPerTicket;

            while (tokens > 0)
            {
                tokens--;

                // Список задач для билета
                var selectedTasks = new List<Task>();

                foreach (var kvp in _tasksPerType)
                {
                    int type = kvp.Key;
                    int count = kvp.Value;

                    if (!tasks.TryGetValue(type, out var candidates) || candidates.Count < count)
                    {
                        selectedTasks.Clear();
                        break; // недостаточно задач для этого типа
                    }

                    var availableTasks = candidates
                        .Where(t => !_usedTaskIds.Contains(t.Id))
                        .ToList();

                    if (availableTasks.Count < count)
                    {
                        selectedTasks.Clear();
                        break; // недостаточно неиспользованных задач
                    }

                    // Случайный выбор 'count' задач для типа
                    var pickedTasks = new List<Task>();
                    for (int i = 0; i < count; i++)
                    {
                        int index = _random.Next(availableTasks.Count);
                        pickedTasks.Add(availableTasks[index]);
                        availableTasks.RemoveAt(index);
                    }

                    selectedTasks.AddRange(pickedTasks);
                }

                if (selectedTasks.Count == 0)
                    continue; // не удалось собрать билет, токен потрачен

                // Проверка разнообразия тем
                var themeGroups = selectedTasks.GroupBy(t => t.Theme);
                if (themeGroups.Count() < 2)
                    continue; // все задания одной темы

                // Проверка суммарной сложности
                int sumComplexity = selectedTasks.Sum(t => t.Complexity);
                if (sumComplexity < _targetDifficulty - _tolerance || sumComplexity > _targetDifficulty + _tolerance)
                    continue; // не удовлетворяет по сложности

                // Билет успешно сгенерирован
                var ticket = new Ticket(ticketNumber++, selectedTasks.Select(t => t.Id).ToList());
                tickets.Add(ticket);

                // Добавляем задачи в список использованных
                foreach (var task in selectedTasks)
                    _usedTaskIds.Add(task.Id);
            }

            return tickets;
        }

        /// <summary>
        /// Генерация всех возможных билетов и запись в файл
        /// </summary>
        public void GenerateAllTickets()
        {
            // Обновление выходного файла
            _ticketWriter.Initialize();

            var filteredTasks = AnalyzeTypes();
            var tickets = Generate(filteredTasks);

            // Запись билетов в файл
            foreach (var ticket in tickets)
                _ticketWriter.AppendTicket(ticket);

            Console.WriteLine($"Сгенерировано {tickets.Count}");
        }
    }
}

