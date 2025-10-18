using System.Collections.Generic;

namespace ClassLibraryTicketGenerator.Models
{
    /// <summary>
    /// Представляет один сгенерированный билет.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Порядковый номер билета (например, «Билет 1»).
        /// </summary>
        public int TicketNumber { get; set; }

        /// <summary>
        /// Список идентификаторов задач, включенных в этот билет.
        /// </summary>
        public List<int> TaskIds { get; set; }

        public Ticket(int ticketNumber, List<int> taskIds)
        {
            TicketNumber = ticketNumber;
            TaskIds = taskIds;
        }
    }
}
