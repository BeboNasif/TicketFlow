using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.BLL.DTOs.Place;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IPlaceService
    {
        Task<IEnumerable<PlaceDto>> GetAllPlacesAsync();
        Task<PlaceDto?> GetPlaceByIdAsync(Guid id);
        Task<PlaceDto> CreatePlaceWithSeatsAsync(CreatePlaceWithSeatsDto dto);
    }
}