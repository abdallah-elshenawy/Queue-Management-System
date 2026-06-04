using QMS.Application.DTOs.Notifications;

namespace QMS.Api.Hubs
{
    /// <summary>
    /// Defines the client-side methods that the server can call.
    /// Your frontend JavaScript must implement these.
    /// </summary>
    public interface IQueueHubClient
    {
        /// <summary>Called when a counter pulls the next ticket.</summary>
        Task TicketCalled(TicketCalledNotification notification);

        /// <summary>Called when a door verifier scans a QR code successfully.</summary>
        Task TicketVerified(TicketVerifiedNotification notification);
    }
}
