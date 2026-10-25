using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.BLL.DTOs.Event;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublished(
            [FromQuery] string? category,
            [FromQuery] DateTime? startDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var events = await _eventService.GetPublishedEventsAsync(category, startDate, page, pageSize);
            return Ok(events);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

        [HttpGet("{id:guid}/seats")]
        public async Task<IActionResult> GetSeatMap(Guid id)
        {
            var seatMap = await _eventService.GetEventSeatMapAsync(id);
            return Ok(seatMap);
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var organizerId))
                return Unauthorized();

            var createdEvent = await _eventService.CreateEventAsync(organizerId, dto);
            return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var organizerId))
                return Unauthorized();

            var success = await _eventService.UpdateEventAsync(id, organizerId, dto);
            if (!success)
                return BadRequest(new { message = "Failed to update event or unauthorized." });

            return Ok(new { message = "Event updated successfully." });
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var organizerId))
                return Unauthorized();

            var success = await _eventService.PublishEventAsync(id, organizerId);
            if (!success)
                return BadRequest(new { message = "Failed to publish event or unauthorized." });

            return Ok(new { message = "Event published successfully." });
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var organizerId))
                return Unauthorized();

            var success = await _eventService.CancelEventAsync(id, organizerId);
            if (!success)
                return BadRequest(new { message = "Failed to cancel event or unauthorized." });

            return Ok(new { message = "Event cancelled successfully." });
        }
    }
}