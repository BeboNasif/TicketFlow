using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface ISeatNotificationService
    {
        Task NotifySeatStatusChangedAsync(Guid eventId, List<Guid> eventSeatIds, string newStatus);
    }
}