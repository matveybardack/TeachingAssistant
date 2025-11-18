using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryTicketGenerator.GLOBAL_Query
{
    public struct Queries
    {
        // Путь к данным
        public const string TasksFilePath = "Data Source=Tasks;Version=3;";
        public const string TicketsFilePath = "tickets.txt";

        /// <summary>
        /// Получение всех задач из Таблицы (id вместо строк)
        /// </summary>
        public const string ReadAllTasks = @"
                SELECT 
                    TaskId,
                    Theme,
                    Type,
                    Difficulty
                FROM Task
            ";

        /// <summary>
        /// Получение задач по из Таблицы
        /// </summary>
        public const string ReadTasksByIds = @"
                SELECT 
                    t.TaskId,
                    th.TaskTheme AS ThemeText,
                    ty.TaskType AS TypeText,
                    t.Difficulty
                FROM Task t
                LEFT JOIN Theme th ON t.Theme = th.ThemeId
                LEFT JOIN Type ty ON t.Type = ty.TypeId
            ";

        /// <summary>
        /// Получение задачи по ID
        /// </summary>
        /// <param name="id">Id задачи</param>
        /// <returns></returns>
        public static string GetTaskById(int id) => $@"
                SELECT t.TaskId, th.TaskTheme AS ThemeText, ty.TaskType AS TypeText, t.Difficulty
                FROM Task t
                LEFT JOIN Theme th ON t.Theme = th.ThemeId
                LEFT JOIN Type ty ON t.Type = ty.TypeId
                WHERE t.TaskId = {id}
            ";

        /// <summary>
        /// Получение типа по ID
        /// </summary>
        /// <param name="id"> Id типа </param>
        /// <returns></returns>
        public static string GetTypeById(int id) => $@"
                SELECT TaskType
                FROM Type
                WHERE TypeId = {id}
            ";
    }
}
