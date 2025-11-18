using ClassLibraryTicketGenerator.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Чтение задач из базы данных SQLite. (только id)
    /// </summary>
    public class TaskReader
    {
        private readonly string _connectionString;

        public TaskReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Ленивая загрузка всех задач.
        /// </summary>
        public IEnumerable<Models.Task> ReadTasks()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = GLOBAL_Query.Queries.ReadAllTasks;

            using var command = new SQLiteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (reader["TaskId"] == DBNull.Value ||
                    reader["Theme"] == DBNull.Value ||
                    reader["Type"] == DBNull.Value ||
                    reader["Difficulty"] == DBNull.Value)
                {
                    continue;
                }

                yield return new Models.Task(
                    id: Convert.ToInt32(reader["TaskId"]),
                    theme: Convert.ToInt32(reader["Theme"]),
                    type: Convert.ToInt32(reader["Type"]),
                    complexity: Convert.ToInt32(reader["Difficulty"])
                );
            }
        }
    }
}
