using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    [Index(nameof(EventId), nameof(SeatId), IsUnique = true)]
    public class EventSeat
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
        public SeatStatusEnum Status { get; set; } = SeatStatusEnum.Available;

        [Timestamp]
        public uint Version { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(SeatId))]
        public Seat Seat { get; set; } = null!;

        public ICollection<ReservationItem> ReservationItems { get; set; } = new List<ReservationItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}