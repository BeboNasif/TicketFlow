using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Event
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Event title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Place ID is required.")]
        public Guid PlaceId { get; set; }
    }

    public class UpdateEventDto : CreateEventDto { }
}