using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(ShopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return categories;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return category == null ? null : _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, CategoryCreateUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            _mapper.Map(dto, category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> ToggleCategoryActiveAsync(int id, bool isActive)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            category.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
