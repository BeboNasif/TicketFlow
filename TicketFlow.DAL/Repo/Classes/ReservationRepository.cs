namespace TicketFlow.DAL.Repo.Classes;

using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(DBContext context) : base(context)
    {
    }

    public async Task<List<Reservation>> GetExpiredPendingReservationsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Status == ReservationStatusEnum.Pending && r.ExpiresAt <= cutoffUtc)
            .Include(r => r.ReservationItems)
                .ThenInclude(ri => ri.EventSeat)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetReservationWithItemsAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Event)
            .Include(r => r.ReservationItems)
                .ThenInclude(ri => ri.EventSeat)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);
    }
}