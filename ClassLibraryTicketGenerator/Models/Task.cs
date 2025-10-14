using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryTicketGenerator.Models
{
    /// <summary>
    /// Представляет метаданные одной задачи, считанные из входного файла.
    /// Полный текст задания не сохраняется для экономии памяти.
    /// </summary>
    public class Task : IComparable<Task>
    {
        /// <summary>
        /// Уникальный идентификатор задачи, номер строки из исходного файла.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тема задания.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Тип задания (например, "Теория", "Практика", "Блитц").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Сложность задания (натуральное число)
        /// </summary>
        public int Complexity { get; set; }

        public Task(int id, string theme, string type, int complexity)
        {
            Id = id;
            Theme = theme;
            Type = type;
            Complexity = complexity;
        }

        public int CompareTo(Task other)
        {
            if (other == null)
                return 1;

            // Основное сравнение — по Id
            int result = Id.CompareTo(other.Id);

            return result;
        }
    }
}
