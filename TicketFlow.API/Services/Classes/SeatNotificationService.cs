using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketFlow.API.Hubs;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.API.Services.Classes
{
    public class SeatNotificationService : ISeatNotificationService
    {
        private readonly IHubContext<SeatHub> _hubContext;

        public SeatNotificationService(IHubContext<SeatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifySeatStatusChangedAsync(Guid eventId, List<Guid> eventSeatIds, string newStatus)
        {
            var groupName = SeatHub.GetGroupName(eventId);

            await _hubContext.Clients.Group(groupName).SendAsync("SeatStatusUpdated", new
            {
                EventId = eventId,
                EventSeatIds = eventSeatIds,
                Status = newStatus
            });
        }
    }
}