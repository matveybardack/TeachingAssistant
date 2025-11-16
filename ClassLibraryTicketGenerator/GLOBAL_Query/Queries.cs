using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryTicketGenerator.GLOBAL_Query
{
    internal struct Queries
    {
        /// <summary>
        /// Получение задач по из Таблицы
        /// </summary>
        internal const string ReadTasksByIds = @"
                SELECT 
                    t.TaskId,
                    th.TaskTheme AS ThemeText,
                    ty.TaskType AS TypeText,
                    t.Difficulty
                FROM Task t
                LEFT JOIN Theme th ON t.Theme = th.Themeld
                LEFT JOIN Type ty ON t.Type = ty.Typeld
            ";

        internal static string GetTaskByIds(string parameters) => $@"
                SELECT t.TaskId, th.TaskTheme AS ThemeText, ty.TaskType AS TypeText, t.Difficulty
                FROM Task t
                LEFT JOIN Theme th ON t.Theme = th.Themeld
                LEFT JOIN Type ty ON t.Type = ty.Typeld
                WHERE t.TaskId IN ({parameters})
            ";
    }
}
