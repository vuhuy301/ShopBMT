using backend_shopcaulong.DTOs.Brand;

namespace backend_shopcaulong.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(int id);
        Task<BrandDto> CreateAsync(BrandCreateDto dto);
        Task<BrandDto?> UpdateAsync(int id, BrandUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
