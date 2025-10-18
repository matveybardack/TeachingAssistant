using ClassLibraryTicketGenerator.Models;
using System.Collections.Generic;
using System.IO;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Чтение и gfhcbyu входного файла.
    /// </summary>
    public class TaskReader
    {
        /// <summary>
        /// Считывает файл задачи построчно и возвращает объекты метаданных задачи.
        /// </summary>
        /// <param name="filePath">Путь к файлу tasks.txt file.</param>
        /// <returns>Множество объектов Task</returns>
        public IEnumerable<Models.Task> ReadTasks(string filePath)
        {
            int currentLine = 1;
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentLine++;
                    continue;
                }

                var parts = line.Split(';');
                if (parts.Length == 4) // Theme;Type;Complexity;Text
                {
                    if (int.TryParse(parts[2].Trim(), out int complexity))
                    {
                        yield return new Models.Task(
                            id: currentLine,
                            theme: parts[0].Trim(),
                            type: parts[1].Trim(),
                            complexity: complexity
                        );
                    }
                }
                // Возможно логирование непрочитанных чтрок
                currentLine++;
            }
        }
    }
}

