using AutoMapper;
using TicketFlow.BLL.DTOs.Category;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var categoryModel = await _unitOfWork.Categories.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(categoryModel);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var categoryModel = await _unitOfWork.Categories.GetByIdAsync(id);
            if (categoryModel == null)
                return false;
            _unitOfWork.Categories.Delete(categoryModel);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}