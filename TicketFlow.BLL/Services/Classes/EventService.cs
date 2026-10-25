using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TicketFlow.BLL.DTOs.Event;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<EventDto>> GetPublishedEventsAsync(string? category, DateTime? startDate, int page, int pageSize)
        {
            var events = await _unitOfWork.Events.GetPublishedEventsAsync(category, startDate, page, pageSize);
            return _mapper.Map<IReadOnlyList<EventDto>>(events);
        }

        public async Task<EventDto?> GetEventByIdAsync(Guid id)
        {
            var ev = await _unitOfWork.Events.GetEventWithDetailsAsync(id);
            return ev == null ? null : _mapper.Map<EventDto>(ev);
        }

        public async Task<IReadOnlyList<EventSeatDto>> GetEventSeatMapAsync(Guid eventId)
        {
            var eventSeats = await _unitOfWork.EventSeats.GetSeatMapByEventIdAsync(eventId);
            return _mapper.Map<IReadOnlyList<EventSeatDto>>(eventSeats);
        }

        public async Task<EventDto> CreateEventAsync(Guid organizerId, CreateEventDto dto)
        {
            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("End time must be after Start time.");

            // 1. Validate place existence using _unitOfWork
            var place = await _unitOfWork.Places.GetByIdAsync(dto.PlaceId);
            if (place == null)
                throw new KeyNotFoundException("Specified place does not exist.");

            // 2. Retrieve physical seats configured for this venue
            var physicalSeats = await _unitOfWork.Seats.FindAsync(s => s.PlaceId == dto.PlaceId);
            if (!physicalSeats.Any())
                throw new InvalidOperationException("Cannot create an event for a venue with no configured seats.");

            var newEvent = _mapper.Map<Event>(dto);
            newEvent.OrganizerId = organizerId;
            newEvent.Status = EventStatusEnum.Draft;

            // 3. Generate event-specific seats from venue template seats
            foreach (var seat in physicalSeats)
            {
                newEvent.EventSeats.Add(new EventSeat
                {
                    SeatId = seat.Id,
                    Price = seat.BasePrice,
                    Status = SeatStatusEnum.Available
                });
            }

            await _unitOfWork.Events.AddAsync(newEvent);
            await _unitOfWork.SaveChangesAsync();

            return (await GetEventByIdAsync(newEvent.Id))!;
        }

        public async Task<bool> UpdateEventAsync(Guid eventId, Guid organizerId, UpdateEventDto dto)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || ev.OrganizerId != organizerId)
                return false;

            if (ev.Status == EventStatusEnum.Cancelled || ev.Status == EventStatusEnum.Completed)
                throw new InvalidOperationException("Cannot modify an event that is cancelled or completed.");

            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("End time must be after Start time.");

            _mapper.Map(dto, ev);
            _unitOfWork.Events.Update(ev);

            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> PublishEventAsync(Guid eventId, Guid organizerId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || ev.OrganizerId != organizerId)
                return false;

            if (ev.Status != EventStatusEnum.Draft)
                throw new InvalidOperationException("Only draft events can be published.");

            ev.Status = EventStatusEnum.Published;
            _unitOfWork.Events.Update(ev);

            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelEventAsync(Guid eventId, Guid organizerId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || ev.OrganizerId != organizerId)
                return false;

            if (ev.Status == EventStatusEnum.Completed)
                throw new InvalidOperationException("Cannot cancel a completed event.");

            ev.Status = EventStatusEnum.Cancelled;
            _unitOfWork.Events.Update(ev);

            return await _unitOfWork.SaveChangesAsync() > 0;
        }
    }
}
