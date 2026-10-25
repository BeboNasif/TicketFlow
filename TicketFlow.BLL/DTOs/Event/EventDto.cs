using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Event
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Guid PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;

        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
    }
}