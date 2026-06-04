using System.Collections.Generic;

namespace QMS.Domain.Models
{
    public class CustomerInfo
    {
        public int UserId { get; set; }

        // FIX: [MaxLength(20)] was placed here — a navigation property is not a column,
        // so the attribute was both meaningless and misleading. Removed.
        public User? User { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
    }
}