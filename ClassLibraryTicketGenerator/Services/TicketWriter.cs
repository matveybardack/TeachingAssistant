using ClassLibraryTicketGenerator.Models;
using ClassLibraryTicketGenerator.GLOBAL_Query;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Запись генерируемых билетов в файл, чтение задач из базы данных.
    /// </summary>
    public class TicketWriter
    {
        private readonly string _outputFilePath;
        private readonly string _connectionString;

        public TicketWriter(string outputFilePath, string connectionString)
        {
            _outputFilePath = outputFilePath;
            _connectionString = connectionString;
        }

        /// <summary>
        /// Очищает выходной файл в начале процесса новой генерации.
        /// </summary>
        public void Initialize()
        {
            File.WriteAllText(_outputFilePath, string.Empty);
        }

        /// <summary>
        /// Добавляет один отформатированный билет в выходной файл.
        /// Читает только необходимые задачи из базы данных по TaskId.
        /// </summary>
        /// <param name="ticket">Билет для записи.</param>
        public void AppendTicket(Ticket ticket)
        {
            var ticketLine = new StringBuilder();
            ticketLine.Append($"Билет {ticket.TicketNumber}; ");

            // Читаем задачи из БД, только нужные TaskId
            var tasksDict = ReadTasksByIds(ticket.TaskIds)
                            .ToDictionary(t => t.Id, t => t);

            foreach (var taskId in ticket.TaskIds)
            {
                if (tasksDict.TryGetValue(taskId, out var task))
                {
                    // Формат: (ID) Тема; Тип; Сложность;
                    ticketLine.Append($"({task.Id}) {task.Theme}; {task.Type}; {task.Complexity}; ");
                }
            }

            File.AppendAllText(_outputFilePath, ticketLine.ToString().TrimEnd(' ', ';') + Environment.NewLine);
        }

        /// <summary>
        /// Чтение задач из БД по списку TaskId.
        /// Пропускает задачи с null в Theme, Type или Difficulty.
        /// </summary>
        private IEnumerable<Models.Task> ReadTasksByIds(IEnumerable<int> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
                yield break;

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // Формируем параметризованный IN для SQLite
            var parameters = string.Join(", ", taskIds.Select((id, idx) => $"@id{idx}"));
            string sql = Queries.GetTaskByIds(parameters);

            using var command = new SQLiteCommand(sql, connection);

            int i = 0;
            foreach (var id in taskIds)
            {
                command.Parameters.AddWithValue($"@id{i++}", id);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader["TaskId"] == DBNull.Value ||
                    reader["ThemeText"] == DBNull.Value ||
                    reader["TypeText"] == DBNull.Value ||
                    reader["Difficulty"] == DBNull.Value)
                {
                    continue;
                }

                yield return new Models.Task(
                    id: Convert.ToInt32(reader["TaskId"]),
                    theme: reader["ThemeText"].ToString().Trim(),
                    type: reader["TypeText"].ToString().Trim(),
                    complexity: Convert.ToInt32(reader["Difficulty"])
                );
            }
        }
    }
}
