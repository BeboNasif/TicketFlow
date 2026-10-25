using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Repo.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<List<Reservation>> GetExpiredPendingReservationsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
        Task<Reservation?> GetReservationWithItemsAsync(Guid reservationId, CancellationToken cancellationToken = default);
    }
}