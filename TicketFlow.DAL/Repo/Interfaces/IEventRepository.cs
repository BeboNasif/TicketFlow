using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Repo.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<Event?> GetEventWithDetailsAsync(Guid eventId, CancellationToken cancellationToken = default); 
        Task<IReadOnlyList<Event>> GetPublishedEventsAsync(
            string? category,
            DateTime? startDate,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}