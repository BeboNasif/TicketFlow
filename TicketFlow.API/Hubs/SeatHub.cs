using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TicketFlow.API.Hubs
{
    public class SeatHub : Hub
    {
        public async Task JoinEventGroup(Guid eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(eventId));
        }

        public async Task LeaveEventGroup(Guid eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(eventId));
        }

        public static string GetGroupName(Guid eventId) => $"Event_{eventId}";
    }
}