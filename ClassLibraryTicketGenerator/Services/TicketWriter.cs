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
            int sumComplexity = 0;

            var ticketLine = new StringBuilder();
            ticketLine.Append($"Билет {ticket.TicketNumber}; ");

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            foreach (var id in ticket.TaskIds)
            {
                string sql = Queries.GetTaskById(id);
                using var command = new SQLiteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                    throw new ArgumentException($"Задача {id} не найдена в БД.");

                ticketLine.Append(
                    $"({reader["TaskId"]}) " +
                    $"{reader["ThemeText"]}; " +
                    $"{reader["TypeText"]}; " +
                    $"{reader["Difficulty"]}; "
                );

                sumComplexity += Convert.ToInt32(reader["Difficulty"]);
            }

            File.AppendAllText(_outputFilePath,
                (ticketLine + sumComplexity.ToString()).ToString().TrimEnd(' ', ';') + Environment.NewLine);
        }
    }
}
