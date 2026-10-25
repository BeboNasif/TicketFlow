using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    [Index(nameof(PlaceId), nameof(Section), nameof(Row), nameof(SeatNumber), IsUnique = true)]
    public class Seat
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PlaceId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Section { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Row { get; set; } = string.Empty;

        public int SeatNumber { get; set; }

        [Precision(18, 2)]
        public decimal BasePrice { get; set; }

        [ForeignKey(nameof(PlaceId))]
        public Place Place { get; set; } = null!;

        public ICollection<EventSeat> EventSeats { get; set; } = new List<EventSeat>();
    }
}