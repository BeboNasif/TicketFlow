namespace TicketFlow.DAL.Repo.Classes;

using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    public EventRepository(DBContext context) : base(context)
    {
    }

    public async Task<Event?> GetEventWithDetailsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Place)
            .Include(e => e.Organizer)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetPublishedEventsAsync(
        string? category,
        DateTime? startDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(e => e.Status == EventStatusEnum.Published)
            .Include(e => e.Place)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(e => e.Category.ToLower() == category.ToLower());
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.StartTime >= startDate.Value);
        }

        return await query
            .OrderBy(e => e.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}