using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid EventSeatId { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(OrderId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Order Order { get; set; } = null!;

        [ForeignKey(nameof(EventSeatId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EventSeat EventSeat { get; set; } = null!;
    }
}