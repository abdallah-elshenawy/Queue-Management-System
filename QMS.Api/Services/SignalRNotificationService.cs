using Microsoft.AspNetCore.SignalR;
using QMS.Api.Hubs;
using QMS.Application.Abstracts;
using QMS.Application.DTOs.Notifications;

namespace QMS.Api.Services
{
    /// <summary>
    /// Implements IQueueNotificationService using SignalR.
    /// Registered in Program.cs — the Application layer never touches SignalR directly.
    /// </summary>
    public class SignalRNotificationService : IQueueNotificationService
    {
        private readonly IHubContext<QueueHub, IQueueHubClient> hubContext;

        public SignalRNotificationService(IHubContext<QueueHub, IQueueHubClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task NotifyTicketCalledAsync(int branchId, TicketCalledNotification notification)
        {
            await hubContext.Clients
                .Group($"branch-{branchId}")
                .TicketCalled(notification);
        }

        public async Task NotifyTicketVerifiedAsync(int branchId, TicketVerifiedNotification notification)
        {
            await hubContext.Clients
                .Group($"branch-{branchId}")
                .TicketVerified(notification);
        }
    }
}
