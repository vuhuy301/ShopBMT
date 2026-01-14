using backend_shopcaulong.DTOs.Promotion;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ShopDbContext _context;

        public PromotionService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAllAsync()
        {
            return await _context.Promotions
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<Promotion> CreateAsync(PromotionCreateDto dto)
        {
            // ✅ Check duplicate Name
            var isDuplicate = await _context.Promotions
                .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

            if (isDuplicate)
                throw new Exception("Ưu đãi đã tồn tại");

            var promotion = new Promotion
            {
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return promotion;
        }

        public async Task<Promotion?> UpdateAsync(int id, PromotionUpdateDto dto)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return null;

            // ✅ Check duplicate (ngoại trừ chính nó)
            var isDuplicate = await _context.Promotions
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == dto.Name.ToLower());

            if (isDuplicate)
                throw new Exception("Tên ưu đãi đã tồn tại");

            promotion.Name = dto.Name.Trim();
            promotion.Description = dto.Description;

            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
