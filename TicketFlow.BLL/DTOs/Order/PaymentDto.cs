using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Order
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}