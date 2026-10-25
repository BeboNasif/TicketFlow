using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Reservation
{
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "Event ID is required.")]
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "At least one seat must be selected.")]
        [MinLength(1, ErrorMessage = "You must select at least 1 seat.")]
        [MaxLength(10, ErrorMessage = "You cannot reserve more than 10 seats per transaction.")]
        public List<Guid> EventSeatIds { get; set; } = new();
    }
}