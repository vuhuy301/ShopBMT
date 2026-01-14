using backend_shopcaulong.DTOs.Promotion;
using backend_shopcaulong.Models;

namespace backend_shopcaulong.Services
{
    public interface IPromotionService
    {
        Task<List<Promotion>> GetAllAsync();
        Task<Promotion?> GetByIdAsync(int id);
        Task<Promotion> CreateAsync(PromotionCreateDto dto);
        Task<Promotion?> UpdateAsync(int id, PromotionUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
