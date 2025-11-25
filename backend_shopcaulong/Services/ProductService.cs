using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;

        private readonly IUploadService _uploadService;
        private readonly IWebHostEnvironment _env; // để xóa file cũ khi update

        public ProductService(
            ShopDbContext context,
            IMapper mapper,
            IUploadService uploadService,
            IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
            _env = env;
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
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                BrandId = dto.BrandId,
                CategoryId = dto.CategoryId,
                IsFeatured = dto.IsFeatured,
                Images = new List<ProductImage>(),
                Details = new List<ProductDetail>(),
                Variants = new List<ProductVariant>()
            };

            // UPLOAD ẢNH
            if (dto.ImageFiles?.Count > 0)
            {
                var urls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                product.Images = urls.Select((url, i) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = i == 0
                }).ToList();
            }

            // Xử lý chi tiết mô tả
            // Trong CreateAsync
            // Thay toàn bộ đoạn foreach thủ công bằng:
            if (dto.Details.Any())
            {
                product.Details = new List<ProductDetail>();
                foreach (var detailDto in dto.Details)
                {
                    string? imageUrl = null;
                    if (detailDto.ImageFile != null)
                    {
                        imageUrl = await _uploadService.UploadDetailImageAsync(detailDto.ImageFile);
                    }

                    product.Details.Add(new ProductDetail
                    {
                        Text = detailDto.Text,
                        ImageUrl = imageUrl,
                        SortOrder = detailDto.SortOrder
                    });
                }
            }

            // Xử lý variant
            if (dto.Variants.Any())
            {
                product.Variants = new List<ProductVariant>();
                foreach (var v in dto.Variants)
                {
                    string? variantImageUrl = null;
                    if (v.ImageFile != null)
                    {
                        variantImageUrl = await _uploadService.UploadVariantImageAsync(v.ImageFile);
                    }

                    product.Variants.Add(new ProductVariant
                    {
                        Size = v.Size,
                        Color = v.Color,
                        Stock = v.Stock,
                        Price = v.Price,
                        DiscountPrice = v.DiscountPrice,
                        MainImageUrl = variantImageUrl
                    });
                }
                product.Stock = product.Variants.Sum(v => v.Stock);
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

            // XỬ LÝ ẢNH MỚI
            if (dto.ImageFiles?.Count > 0)
            {
                // Xóa file cũ trên server
                foreach (var img in product.Images)
                {
                    var filePath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                // Upload ảnh mới
                var newUrls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                _context.ProductImages.RemoveRange(product.Images);

                product.Images = newUrls.Select((url, i) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    ProductId = product.Id
                }).ToList();
            }

            // Cập nhật chi tiết
           if (dto.Details != null)
            {
                // Xóa ảnh cũ
                foreach (var old in product.Details)
                {
                    if (!string.IsNullOrEmpty(old.ImageUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, old.ImageUrl.TrimStart('/').Replace("/images/products/details/", ""));
                        var fullPath = Path.Combine(_env.WebRootPath, "images", "products", "details", Path.GetFileName(oldPath));
                        if (System.IO.File.Exists(fullPath))
                            System.IO.File.Delete(fullPath);
                    }
                }

                _context.ProductDetails.RemoveRange(product.Details);
                product.Details = new List<ProductDetail>();

                foreach (var d in dto.Details)
                {
                    string? url = null;
                    if (d.ImageFile != null)
                    {
                        url = await _uploadService.UploadDetailImageAsync(d.ImageFile);
                    }

                    product.Details.Add(new ProductDetail
                    {
                        Text = d.Text,
                        ImageUrl = url,
                        SortOrder = d.SortOrder,
                        ProductId = product.Id
                    });
                }
            }

            // Cập nhật variant
            // 4. XỬ LÝ VARIANTS + ẢNH RIÊNG CHO TỪNG BIẾN THỂ (BỔ SUNG QUAN TRỌNG NHẤT!)
            if (dto.Variants != null)
            {
                // Xóa ảnh cũ của các variant (nếu có)
                foreach (var oldVariant in product.Variants.ToList())
                {
                    if (!string.IsNullOrEmpty(oldVariant.MainImageUrl))
                    {
                        var filePath = Path.Combine(_env.WebRootPath, "images", "products", "variants",
                            Path.GetFileName(oldVariant.MainImageUrl));
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                }

                _context.ProductVariants.RemoveRange(product.Variants);
                product.Variants = new List<ProductVariant>();

                foreach (var v in dto.Variants)
                {
                    string? variantImageUrl = null;
                    if (v.ImageFile != null && v.ImageFile.Length > 0)
                    {
                        variantImageUrl = await _uploadService.UploadVariantImageAsync(v.ImageFile);
                    }

                    product.Variants.Add(new ProductVariant
                    {
                        Size = v.Size,
                        Color = v.Color,
                        Stock = v.Stock,
                        Price = v.Price,
                        DiscountPrice = v.DiscountPrice,
                        MainImageUrl = variantImageUrl, // ← ẢNH RIÊNG CHO MÀU NÀY
                        ProductId = product.Id
                    });
                }

                // Tính lại tổng stock
                product.Stock = product.Variants.Sum(v => v.Stock);
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
        public async Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.Variants)
                .AsNoTracking()
                .OrderBy(p => p.Name);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResultDto<ProductDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = items
            };
        }

    }
}
