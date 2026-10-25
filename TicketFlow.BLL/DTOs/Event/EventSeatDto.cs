using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Event
{
    public class EventSeatDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public string Section { get; set; } = string.Empty;
        public string Row { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}