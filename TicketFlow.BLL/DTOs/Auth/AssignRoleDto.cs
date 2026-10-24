using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Auth
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [RegularExpression("^(Admin|Organizer|Customer)$", ErrorMessage = "Role must be Admin, Organizer, or Customer.")]
        public string RoleName { get; set; } = string.Empty;
    }
}