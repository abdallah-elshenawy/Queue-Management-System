

using QMS.Application.DTOs.Notifications;

namespace QMS.Application.Abstracts
{
    /// <summary>
    /// Decouples TicketService from SignalR.
    /// The Application layer only knows about this interface.
    /// The API layer provides the SignalR implementation.
    /// </summary>
    public interface IQueueNotificationService
    {
        /// <summary>Broadcast to all clients in the branch group when a ticket is called.</summary>
        Task NotifyTicketCalledAsync(int branchId, TicketCalledNotification notification);

        /// <summary>Broadcast to all clients in the branch group when a ticket is verified at the door.</summary>
        Task NotifyTicketVerifiedAsync(int branchId, TicketVerifiedNotification notification);
    }
}
