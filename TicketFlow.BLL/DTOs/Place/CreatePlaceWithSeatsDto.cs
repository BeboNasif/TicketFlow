using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Place
{
    public class CreatePlaceWithSeatsDto
    {
        [Required(ErrorMessage = "Place name is required.")]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string DefaultSection { get; set; } = "Main Floor";

        [Required]
        [Range(1, 26, ErrorMessage = "Total rows must be between 1 and 26.")]
        public int TotalRows { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Seats per row must be between 1 and 100.")]
        public int SeatsPerRow { get; set; }

        [Range(0, 10000, ErrorMessage = "Base price must be a positive value.")]
        public decimal DefaultBasePrice { get; set; } = 50.00m;
    }
}