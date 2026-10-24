using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Repo.Interfaces
{
    public interface IEventSeatRepository
    {
        Task<IReadOnlyList<EventSeat>> GetSeatMapByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<List<EventSeat>> GetSeatsForHoldAsync(Guid eventId, List<Guid> seatIds, CancellationToken cancellationToken = default);
        Task<bool> AreSeatsAvailableAsync(Guid eventId, List<Guid> seatIds, CancellationToken cancellationToken = default);
    }
}
