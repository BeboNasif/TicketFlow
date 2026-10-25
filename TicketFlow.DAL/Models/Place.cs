using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public class Place
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}