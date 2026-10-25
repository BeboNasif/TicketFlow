using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.BLL.DTOs.Category;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _category;
        public CategoryController(ICategoryService category)
        {
            _category = category;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _category.GetAllCategoriesAsync();
            return Ok(categories);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _category.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var created = await _category.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _category.DeleteCategoryAsync(id);
            if (!success)
                return NotFound(new { message = "Category not found." });

            return NoContent();
        }
    }
}