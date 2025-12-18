using backend_shopcaulong.DTOs.Banner;

namespace backend_shopcaulong.Services
{
   public interface IBannerService
{
    Task<List<BannerDto>> GetAllAsync();
    Task<BannerDto?> GetByIdAsync(int id);
    Task<BannerDto> CreateAsync(CreateBannerDto dto);
    Task<bool> UpdateAsync(int id, UpdateBannerDto dto);
    Task<bool> DeleteAsync(int id);
}

}