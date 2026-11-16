using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Order
{
    public class CreateOrderDto
    {

        [Required(ErrorMessage = "Reservation ID is required.")]
        public Guid ReservationId { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public string PaymentMethod { get; set; } = "CreditCard";
    }
}