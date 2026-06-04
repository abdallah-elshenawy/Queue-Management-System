using Microsoft.AspNetCore.SignalR;

namespace QMS.Api.Hubs
{
    /// <summary>
    /// Clients connect here and join the group for their branch.
    /// The server then pushes events to that group.
    ///
    /// Group naming convention: "branch-{branchId}"
    /// e.g. a display screen for branch 3 calls JoinBranch(3)
    /// </summary>
    public class QueueHub : Hub<IQueueHubClient>
    {
        /// <summary>
        /// Called by display screens and counter clients on connect.
        /// Adds the connection to the branch-specific group.
        /// </summary>
        public async Task JoinBranch(int branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }

        /// <summary>
        /// Called when a client navigates away or disconnects cleanly.
        /// </summary>
        public async Task LeaveBranch(int branchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Groups are cleaned up automatically by SignalR on disconnect,
            // but calling base ensures proper lifecycle handling.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
