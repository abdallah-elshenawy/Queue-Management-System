namespace QMS.Application.DTOs.Ticket
{
    public class GetTicket
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string TicketNumber { get; set; }

        // FIX: was = Guid.NewGuid().ToString("N") — a DTO should never generate its own data;
        // these values must come from the mapped Ticket entity.
        public string QRCodeData { get; set; }
        public string Status { get; set; }
        public DateTime? ReservedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? IsVerifiedAtDoor { get; set; }
        public DateTime? VerificationTime { get; set; }

        public string BranchName { get; set; }
        public string ServiceName { get; set; }
        public string EmployeeName { get; set; }
    }
}