using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(ShopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.Variants)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return products;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.Variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);

            // Xử lý ảnh chính (nếu có)
            if (dto.ImageUrls.Any())
            {
                product.Images = dto.ImageUrls.Select((url, index) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0
                }).ToList();
            }

            // Xử lý chi tiết mô tả
            if (dto.Details.Any())
            {
                product.Details = dto.Details.Select(d => new ProductDetail
                {
                    Text = d.Text,
                    ImageUrl = d.ImageUrl,
                    SortOrder = d.SortOrder
                }).ToList();
            }

            // Xử lý variant
            if (dto.Variants.Any())
            {
                product.Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Color = v.Color,
                    Stock = v.Stock,
                    Price = v.Price,
                    DiscountPrice = v.DiscountPrice
                }).ToList();
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            _mapper.Map(dto, product);

            // Cập nhật ảnh (xóa cũ, thêm mới)
            if (dto.ImageUrls != null)
            {
                _context.ProductImages.RemoveRange(product.Images);
                product.Images = dto.ImageUrls.Select((url, index) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0,
                    ProductId = product.Id
                }).ToList();
            }

            // Cập nhật chi tiết
            if (dto.Details != null)
            {
                _context.ProductDetails.RemoveRange(product.Details);
                product.Details = dto.Details.Select(d => new ProductDetail
                {
                    Text = d.Text,
                    ImageUrl = d.ImageUrl,
                    SortOrder = d.SortOrder,
                    ProductId = product.Id
                }).ToList();
            }

            // Cập nhật variant
            if (dto.Variants != null)
            {
                _context.ProductVariants.RemoveRange(product.Variants);
                product.Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Color = v.Color,
                    Stock = v.Stock,
                    Price = v.Price,
                    DiscountPrice = v.DiscountPrice,
                    ProductId = product.Id
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
