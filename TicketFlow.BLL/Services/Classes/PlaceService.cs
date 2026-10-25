using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TicketFlow.BLL.DTOs.Place;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class PlaceService : IPlaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlaceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlaceDto>> GetAllPlacesAsync()
        {
            var places = await _unitOfWork.Places.GetAllAsync();
            return _mapper.Map<IEnumerable<PlaceDto>>(places);
        }

        public async Task<PlaceDto?> GetPlaceByIdAsync(Guid id)
        {
            var place = await _unitOfWork.Places.GetByIdAsync(id);
            return _mapper.Map<PlaceDto>(place);
        }

        public async Task<PlaceDto> CreatePlaceWithSeatsAsync(CreatePlaceWithSeatsDto dto)
        {
            var place = new Place
            {
                Name = dto.Name,
                Address = dto.Address,
                Capacity = dto.TotalRows * dto.SeatsPerRow
            };

            for (int rowIdx = 0; rowIdx < dto.TotalRows; rowIdx++)
            {
                char rowChar = (char)('A' + rowIdx);
                for (int seatNum = 1; seatNum <= dto.SeatsPerRow; seatNum++)
                {
                    place.Seats.Add(new Seat
                    {
                        Section = dto.DefaultSection,
                        Row = rowChar.ToString(),
                        SeatNumber = seatNum,
                        BasePrice = dto.DefaultBasePrice
                    });
                }
            }

            await _unitOfWork.Places.AddAsync(place);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PlaceDto>(place);
        }
    }
}