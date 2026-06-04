namespace QMS.Application.DTOs.DisplayScreen
{
    public class GetDisplayScreen
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
        public DateTime? LastUpdated { get; set; }

        // FIX: renamed from getDisplayTickets (camelCase on a public property violates C# conventions)
        public ICollection<GetDisplayTicket>? DisplayTickets { get; set; }
    }
}