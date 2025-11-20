using backend_shopcaulong.DTOs.Category;

namespace backend_shopcaulong.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto dto);
        Task<CategoryDto?> UpdateAsync(int id, CategoryCreateUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
