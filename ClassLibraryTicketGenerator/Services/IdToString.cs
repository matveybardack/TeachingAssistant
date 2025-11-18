using ClassLibraryTicketGenerator.GLOBAL_Query;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryTicketGenerator.Services
{
    internal static class IdToString
    {
        /// <summary>
        /// Получение типа задачи по её идентификатору.
        /// </summary>
        /// <param name="id"> Id типа </param>
        /// <param name="type"> имя типа </param>
        /// <returns> true, если тип найден, иначе false </returns>
        public static bool TryConvertTypeIdToString(int id, out string type)
        {
            //начальная инициализация
            type = string.Empty;

            using var connection = new SQLiteConnection(Queries.TasksFilePath);
            connection.Open();

            string sql = Queries.GetTypeById(id);
            using var command = new SQLiteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return false;

            type = reader["TaskType"].ToString().Trim();

            return true;
        }
    }
}
