using ClassLibraryTicketGenerator.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Чтение задач из базы данных SQLite с учетом нормализованных справочников Theme и Type.
    /// </summary>
    public class TaskReader
    {
        private readonly string _connectionString;

        public TaskReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Возвращает задачи с текстами Theme и Type. Пропускает строки, где любое значение null.
        /// </summary>
        public IEnumerable<Models.Task> ReadTasks()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = GLOBAL_Query.Queries.ReadTasksByIds;

            using var command = new SQLiteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                // Null проверка
                if (reader["TaskId"] == DBNull.Value ||
                    reader["ThemeText"] == DBNull.Value ||
                    reader["TypeText"] == DBNull.Value ||
                    reader["Difficulty"] == DBNull.Value)
                {
                    continue;
                }

                int taskId = Convert.ToInt32(reader["TaskId"]);
                string theme = reader["ThemeText"].ToString().Trim();
                string type = reader["TypeText"].ToString().Trim();
                int difficulty = Convert.ToInt32(reader["Difficulty"]);

                yield return new Models.Task(
                    id: taskId,
                    theme: theme,
                    type: type,
                    complexity: difficulty
                );
            }
        }
    }
}
