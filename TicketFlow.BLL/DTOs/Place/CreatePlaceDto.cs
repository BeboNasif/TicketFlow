using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Place
{
    public class CreatePlaceDto
    {
        [Required(ErrorMessage = "Place name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Place name must be between 2 and 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 1000000, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }
    }

    public class UpdatePlaceDto : CreatePlaceDto { }
}
