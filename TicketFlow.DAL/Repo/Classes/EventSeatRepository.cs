namespace TicketFlow.DAL.Repo.Classes;

using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

public class EventSeatRepository : GenericRepository<EventSeat>, IEventSeatRepository
{
    public EventSeatRepository(DBContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<EventSeat>> GetSeatMapByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(es => es.EventId == eventId)
            .Include(es => es.Seat)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EventSeat>> GetSeatsForHoldAsync(Guid eventId, List<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        // EF Core Change Tracker stays enabled here (no AsNoTracking) so updates persist on SaveChanges
        return await _dbSet
                .Where(es => es.EventId == eventId
                        && seatIds.Contains(es.Id)
                        && es.Status == SeatStatusEnum.Available)
                .ToListAsync(cancellationToken);
    }

    public async Task<bool> AreSeatsAvailableAsync(Guid eventId, List<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        var seats = await _dbSet
            .Where(es => es.EventId == eventId && seatIds.Contains(es.SeatId))
            .Select(es => es.Status)
            .ToListAsync(cancellationToken);

        return seats.Count == seatIds.Count && seats.All(status => status == SeatStatusEnum.Available);
    }
}