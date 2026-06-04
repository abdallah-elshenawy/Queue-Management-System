namespace QMS.Domain.Models
{
    public class DisplayScreen
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool? IsActive { get; set; } = true;

        // Navigation Properties
        // FIX: added Branch navigation property — required for the explicit 1-to-1
        // relationship configured in ApplicationDbContext (Branch.HasOne(d => d.Display).WithOne(d => d.Branch))
        public Branch? Branch { get; set; }
        public ICollection<DisplayTicket>? DisplayTickets { get; set; }
    }
}