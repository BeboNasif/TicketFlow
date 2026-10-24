using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid? ReservationId { get; set; }

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AppUser User { get; set; } = null!;

        [ForeignKey(nameof(ReservationId))]
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public Reservation? Reservation { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
    }
}