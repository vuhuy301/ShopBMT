using AutoMapper;
using backend_shopcaulong.DTOs.Brand;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class BrandService : IBrandService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;

        public BrandService(ShopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ToListAsync();

            return _mapper.Map<List<BrandDto>>(brands);
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            return brand == null ? null : _mapper.Map<BrandDto>(brand);
        }

        public async Task<BrandDto> CreateAsync(BrandCreateDto dto)
        {
            var brand = _mapper.Map<Brand>(dto);
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<BrandDto?> UpdateAsync(int id, BrandUpdateDto dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return null;

            _mapper.Map(dto, brand);
            await _context.SaveChangesAsync();

            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null) return false;

            // Không cho xóa nếu Brand còn sản phẩm
            if (brand.Products.Any()) return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
