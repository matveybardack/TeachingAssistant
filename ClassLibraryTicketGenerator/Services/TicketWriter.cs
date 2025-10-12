using ClassLibraryTicketGenerator.Models;
using System.Collections.Generic;
using System.IO;

namespace ClassLibraryTicketGenerator.Services
{
    /// <summary>
    /// Запись генерируемых билетов в файл
    /// </summary>
    public class TicketWriter
    {
        private readonly string _outputFilePath;
        private readonly string _tasksFilePath;

        public TicketWriter(string outputFilePath, string tasksFilePath)
        {
            _outputFilePath = outputFilePath;
            _tasksFilePath = tasksFilePath;
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
        /// Эта реализация перечитывает только необходимые строки из файла задачи, чтобы получить полный текст задачи
        /// </summary>
        /// <param name="ticket">Билет для записи.</param>
        public void AppendTicket(Ticket ticket)
        {
            var ticketLine = new System.Text.StringBuilder();
            ticketLine.Append($"Билет {ticket.TicketNumber}; ");

            var linesToRead = new HashSet<int>(ticket.TaskIds);
            var relevantLines = File.ReadLines(_tasksFilePath)
                                    .Select((line, index) => new { Line = line, Index = index + 1 })
                                    .Where(x => linesToRead.Contains(x.Index))
                                    .ToDictionary(x => x.Index, x => x.Line);

            foreach (var taskId in ticket.TaskIds)
            {
                if (relevantLines.TryGetValue(taskId, out var line))
                {
                    var parts = line.Split(';');
                    if (parts.Length == 4)
                    {
                        // Format: (ID) Тема; Тип; Сложность; Текст;
                        ticketLine.Append($"({taskId}) {parts[0].Trim()}; {parts[1].Trim()}; {parts[2].Trim()}; {parts[3].Trim()}; ");
                    }
                }
            }

            File.AppendAllText(_outputFilePath, ticketLine.ToString().TrimEnd(' ', ';') + System.Environment.NewLine);
        }
    }
}
