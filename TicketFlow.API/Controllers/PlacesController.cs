using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.BLL.DTOs.Place;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlacesController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var places = await _placeService.GetAllPlacesAsync();
            return Ok(places);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var place = await _placeService.GetPlaceByIdAsync(id);
            if (place == null)
                return NotFound(new { message = "Place not found." });

            return Ok(place);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateWithSeats([FromBody] CreatePlaceWithSeatsDto dto)
        {
            var createdPlace = await _placeService.CreatePlaceWithSeatsAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPlace.Id }, createdPlace);
        }
    }
}