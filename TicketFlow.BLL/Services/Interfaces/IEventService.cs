using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.BLL.DTOs.Event;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IEventService
    {
        Task<IReadOnlyList<EventDto>> GetPublishedEventsAsync(string? category, DateTime? startDate, int page, int pageSize);
        Task<EventDto?> GetEventByIdAsync(Guid id);
        Task<IReadOnlyList<EventSeatDto>> GetEventSeatMapAsync(Guid eventId);
        Task<EventDto> CreateEventAsync(Guid organizerId, CreateEventDto dto);
        Task<bool> UpdateEventAsync(Guid eventId, Guid organizerId, UpdateEventDto dto);
        Task<bool> PublishEventAsync(Guid eventId, Guid organizerId);
        Task<bool> CancelEventAsync(Guid eventId, Guid organizerId);
    }
}