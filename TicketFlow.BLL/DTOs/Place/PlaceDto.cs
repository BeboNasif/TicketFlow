using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.DTOs.Place
{
    public class PlaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}