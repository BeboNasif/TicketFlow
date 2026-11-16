using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    [Index(nameof(Status), nameof(ExpiresAt))]
    public class Reservation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public ReservationStatusEnum Status { get; set; } = ReservationStatusEnum.Pending;
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = null!;

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        public ICollection<ReservationItem> ReservationItems { get; set; } = new List<ReservationItem>();
        public Order? Order { get; set; }
    }
}