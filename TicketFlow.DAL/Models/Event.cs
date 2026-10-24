using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public class Event
    {
        [Key]
        public Guid Id{ get; set; } = Guid.NewGuid();
        public Guid PlaceId { get; set; }
        public Guid OrganizerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EventStatusEnum Status = EventStatusEnum.Draft;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        [ForeignKey(nameof(PlaceId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Place Place { get; set; } = null!;

        [ForeignKey(nameof(OrganizerId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AppUser Organizer { get; set; } = null!;
        public ICollection<EventSeat> EventSeats { get; set; } = new List<EventSeat>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}