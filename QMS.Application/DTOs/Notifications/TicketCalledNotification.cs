

namespace QMS.Application.DTOs.Notifications
{
    public class TicketCalledNotification
    {
        public string TicketNumber { get; set; }
        public int? CounterNumber { get; set; }
        public string ServiceName { get; set; }
        public DateTime CalledAt { get; set; }
    }
}
