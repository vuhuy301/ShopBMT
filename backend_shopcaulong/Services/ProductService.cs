// Services/ProductService.cs
using AutoMapper;
using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace backend_shopcaulong.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;

        public ProductService(ShopDbContext context, IMapper mapper, IUploadService uploadService)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
        }

        // Mapping chuẩn EF Core – trả về đầy đủ ảnh màu
        private static readonly Expression<Func<Product, ProductDto>> ToProductDto = p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            BrandId = p.BrandId,
            BrandName = p.Brand != null ? p.Brand.Name : null,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : null,
            IsFeatured = p.IsFeatured,
            Stock = p.ColorVariants != null && p.ColorVariants.Any()
                ? p.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock)
                : 0,
            Images = p.Images != null
                ? p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
                : new List<ProductImageDto>(),
            Details = p.Details != null
                ? p.Details.Select(d => new ProductDetailDto
                {
                    Id = d.Id,
                    Text = d.Text,
                    ImageUrl = d.ImageUrl,
                    SortOrder = d.SortOrder
                }).ToList()
                : new List<ProductDetailDto>(),
            ColorVariants = p.ColorVariants != null
                ? p.ColorVariants.Select(cv => new ColorVariantDto
                {
                    Id = cv.Id,
                    Color = cv.Color,
                    ImageUrls = cv.Images != null
                        ? cv.Images.Select(i => i.ImageUrl).ToList()
                        : new List<string>(),
                    Sizes = cv.Sizes != null
                        ? cv.Sizes.Select(s => new SizeVariantDto
                        {
                            Id = s.Id,
                            Size = s.Size,
                            Stock = s.Stock,
                            Price = s.Price
                        }).ToList()
                        : new List<SizeVariantDto>()
                }).ToList()
                : new List<ColorVariantDto>()
        };

        private IQueryable<Product> GetProductQuery()
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Images)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Sizes)
                .AsNoTracking();
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
            => await GetProductQuery().OrderBy(p => p.Name).Select(ToProductDto).ToListAsync();

        // ĐÃ SỬA: Dùng ToProductDto → ảnh màu hiện 100%
        public async Task<ProductDto?> GetByIdAsync(int id)
            => await GetProductQuery()
                .Where(p => p.Id == id)
                .Select(ToProductDto)
                .FirstOrDefaultAsync();

        public async Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize)
        {
            var query = GetProductQuery().OrderBy(p => p.Name);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(ToProductDto).ToListAsync();

            return new PagedResultDto<ProductDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsByFilterAsync(
            int? categoryId, int? brandId, string? search, string? sortBy, int page, int pageSize)
        {
            var query = GetProductQuery();

            if (categoryId.HasValue && categoryId > 0) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (brandId.HasValue && brandId > 0) query = query.Where(p => p.BrandId == brandId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.Trim().ToLower()));

            query = sortBy switch
            {
                "asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(ToProductDto).ToListAsync();

            return new PagedResultDto<ProductDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        // ĐÃ SỬA: Dùng ToProductDto thay vì _mapper.Map
        public async Task<List<ProductDto>> GetTopNewByCategoryAsync(int categoryId)
            => await GetProductQuery()
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(ToProductDto)
                .ToListAsync();
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
                ColorVariants = new List<ProductColorVariant>()
            };

            // 1. TẠO PRODUCT TRƯỚC → LẤY ID THẬT
            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // ← DÒNG QUAN TRỌNG NHẤT!!!

            // BÂY GIỜ product.Id ĐÃ CÓ GIÁ TRỊ THẬT (ví dụ: 15)

            // Ảnh chính sản phẩm
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                var urls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                product.Images = urls.Select((url, i) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    ProductId = product.Id // ← BÂY GIỜ MỚI AN TOÀN!
                }).ToList();
            }

            // Chi tiết mô tả
            if (dto.Details?.Any() == true)
            {
                foreach (var d in dto.Details)
                {
                    string? imgUrl = null;
                    if (d.ImageFile != null)
                        imgUrl = await _uploadService.UploadDetailImageAsync(d.ImageFile);

                    product.Details.Add(new ProductDetail
                    {
                        Text = d.Text,
                        ImageUrl = imgUrl,
                        SortOrder = d.SortOrder,
                        ProductId = product.Id
                    });
                }
            }

            // Biến thể màu – BÂY GIỜ DÙNG product.Id LÀ AN TOÀN
            if (dto.ColorVariants?.Any() == true)
            {
                foreach (var cvDto in dto.ColorVariants)
                {
                    var colorVariant = new ProductColorVariant
                    {
                        Color = cvDto.Color,
                        Images = new List<ProductImage>(),
                        Sizes = new List<ProductSizeVariant>()
                    };

                    if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                    {
                        var urls = await _uploadService.UploadProductImagesAsync(cvDto.ImageFiles);
                        colorVariant.Images = urls.Select((url, i) => new ProductImage
                        {
                            ImageUrl = url,
                            IsPrimary = i == 0,
                            ProductId = product.Id // ← BÂY GIỜ ĐÚNG 100%!
                        }).ToList();
                    }

                    foreach (var s in cvDto.Sizes)
                    {
                        colorVariant.Sizes.Add(new ProductSizeVariant
                        {
                            Size = s.Size,
                            Stock = s.Stock,
                            Price = s.Price ?? dto.Price
                        });
                    }

                    product.ColorVariants.Add(colorVariant);
                }
            }

            // Cập nhật stock
            product.Stock = product.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock);

            // Lưu lại lần cuối
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        // ============================ UPDATE ============================
        public async Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Images)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            // Cập nhật thông tin cơ bản
            if (dto.Name != null) product.Name = dto.Name;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price != null) product.Price = dto.Price.Value;
            if (dto.DiscountPrice != null) product.DiscountPrice = dto.DiscountPrice;
            if (dto.BrandId != null) product.BrandId = dto.BrandId.Value;
            if (dto.CategoryId != null) product.CategoryId = dto.CategoryId.Value;
            if (dto.IsFeatured != null) product.IsFeatured = dto.IsFeatured.Value;

            // Ảnh chính
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                foreach (var img in product.Images)
                    _uploadService.DeleteFile(img.ImageUrl);

                _context.ProductImages.RemoveRange(product.Images);

                var urls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                product.Images = urls.Select((url, i) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    ProductId = product.Id
                }).ToList();
            }

            // Chi tiết mô tả
            if (dto.Details != null)
            {
                foreach (var d in product.Details.Where(x => x.ImageUrl != null))
                    _uploadService.DeleteFile(d.ImageUrl!);

                _context.ProductDetails.RemoveRange(product.Details);

                foreach (var d in dto.Details)
                {
                    string? url = null;
                    if (d.ImageFile != null)
                        url = await _uploadService.UploadDetailImageAsync(d.ImageFile);

                    product.Details.Add(new ProductDetail
                    {
                        Text = d.Text,
                        ImageUrl = url,
                        SortOrder = d.SortOrder,
                        ProductId = product.Id
                    });
                }
            }

            // Biến thể màu – XỬ LÝ ĐÚNG VỚI DTO HIỆN TẠI
            if (dto.ColorVariants != null)
            {
                // XÓA CŨ
                foreach (var cv in product.ColorVariants.ToList())
                {
                    foreach (var img in cv.Images)
                        _uploadService.DeleteFile(img.ImageUrl);

                    _context.ProductImages.RemoveRange(cv.Images);
                    _context.ProductSizeVariants.RemoveRange(cv.Sizes);
                }
                _context.ProductColorVariants.RemoveRange(product.ColorVariants);
                product.ColorVariants.Clear();

                // THÊM MỚI
                // THÊM MỚI – ĐÃ SỬA HOÀN CHỈNH
                foreach (var cvDto in dto.ColorVariants)
                {
                    var colorVariant = new ProductColorVariant
                    {
                        Color = cvDto.Color,
                        Images = new List<ProductImage>(),
                        Sizes = new List<ProductSizeVariant>()
                    };

                    // 1. Giữ lại ảnh cũ (nếu frontend gửi về ImageUrls)
                    if (cvDto.ImageUrls != null && cvDto.ImageUrls.Any())
                    {
                        foreach (var url in cvDto.ImageUrls)
                        {
                            colorVariant.Images.Add(new ProductImage
                            {
                                ImageUrl = url,
                                IsPrimary = false,
                                ProductId = product.Id   // ← CẦN CÓ
                            });
                        }
                    }

                    // 2. Upload ảnh mới
                    if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                    {
                        var newUrls = await _uploadService.UploadProductImagesAsync(cvDto.ImageFiles);
                        foreach (var url in newUrls)
                        {
                            colorVariant.Images.Add(new ProductImage
                            {
                                ImageUrl = url,
                                IsPrimary = false,
                                ProductId = product.Id   // ← BẮT BUỘC PHẢI CÓ DÒNG NÀY!
                            });
                        }
                    }

                    // 3. Size
                    foreach (var s in cvDto.Sizes)
                    {
                        colorVariant.Sizes.Add(new ProductSizeVariant
                        {
                            Size = s.Size,
                            Stock = s.Stock,
                            Price = s.Price ?? product.Price
                        });
                    }

                    product.ColorVariants.Add(colorVariant);
                }   

                // Cập nhật tổng stock
                product.Stock = product.ColorVariants
                    .SelectMany(cv => cv.Sizes)
                    .Sum(s => s.Stock);
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(product);
        }

        // ============================ DELETE ============================
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Details)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Images)
                .Include(p => p.ColorVariants!)
                    .ThenInclude(cv => cv.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return false;

            foreach (var img in product.Images)
                _uploadService.DeleteFile(img.ImageUrl);

            foreach (var cv in product.ColorVariants)
                foreach (var img in cv.Images)
                    _uploadService.DeleteFile(img.ImageUrl);

            foreach (var d in product.Details.Where(d => d.ImageUrl != null))
                _uploadService.DeleteFile(d.ImageUrl!);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}