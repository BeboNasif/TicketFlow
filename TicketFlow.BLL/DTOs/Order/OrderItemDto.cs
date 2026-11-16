using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Order
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid EventSeatId { get; set; }
        public string Section { get; set; } = string.Empty;
        public string Row { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public decimal UnitPrice { get; set; }
    }
}