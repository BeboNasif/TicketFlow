using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public PaymentDto? Payment { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}