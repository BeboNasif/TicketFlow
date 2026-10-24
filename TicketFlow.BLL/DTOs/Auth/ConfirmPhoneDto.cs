using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Auth
{
    public class ConfirmPhoneDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "Verification code must be between 4 and 6 characters.")]
        public string Code { get; set; } = string.Empty;
    }
}