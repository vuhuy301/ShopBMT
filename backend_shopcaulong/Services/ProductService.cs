
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
        private readonly IAiSyncService _aiSync;

        public ProductService(ShopDbContext context, IMapper mapper, IUploadService uploadService , IAiSyncService aiSync)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
            _aiSync = aiSync;
        }

        // Mapping chuẩn – DỰA HOÀN TOÀN VÀO ColorVariantId → CHUẨN SHOPEE
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
                ? p.Images
                    .Where(i => i.ColorVariantId == null && !i.ImageUrl.Contains("/details/"))
                    .Select(i => new ProductImageDto
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

        public async Task<ProductDto?> GetByIdAsync(int id)
            => await GetProductQuery().Where(p => p.Id == id).Select(ToProductDto).FirstOrDefaultAsync();

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

        public async Task<List<ProductDto>> GetTopNewByCategoryAsync(int categoryId)
            => await GetProductQuery()
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(ToProductDto)
                .ToListAsync();

        // CREATE – HOÀN HẢO
        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            if (dto.ColorVariants == null || !dto.ColorVariants.Any())
                throw new ArgumentException("Sản phẩm phải có ít nhất 1 biến thể màu và size.");

            foreach (var cv in dto.ColorVariants)
                if (cv.Sizes == null || !cv.Sizes.Any())
                    throw new ArgumentException($"Biến thể màu '{cv.Color}' phải có ít nhất 1 size.");

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

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // để có product.Id

            // Ảnh chính
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                var urls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                product.Images = urls.Select((url, i) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    ProductId = product.Id
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

            // Biến thể màu
            foreach (var cvDto in dto.ColorVariants!)
            {
                var cv = new ProductColorVariant
                {
                    Color = cvDto.Color,
                    ProductId = product.Id,
                    Images = new List<ProductImage>(),
                    Sizes = new List<ProductSizeVariant>()
                };

                if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                {
                    var urls = await _uploadService.UploadVariantImagesAsync(cvDto.ImageFiles);
                    foreach (var url in urls)
                    {
                        cv.Images.Add(new ProductImage
                        {
                            ImageUrl = url,
                            ProductId = product.Id,
                            ColorVariantId = cv.Id,
                            IsPrimary = false
                        });
                    }
                }

                foreach (var s in cvDto.Sizes)
                {
                    cv.Sizes.Add(new ProductSizeVariant
                    {
                        Size = s.Size,
                        Stock = s.Stock,
                        ColorVariantId = cv.Id
                    });
                }

                product.ColorVariants.Add(cv);
            }

            product.Stock = product.ColorVariants.SelectMany(c => c.Sizes).Sum(s => s.Stock);
            await _context.SaveChangesAsync();
            var productDto = await GetByIdAsync(product.Id)
                ?? throw new Exception("Tạo thất bại");

            _ = Task.Run(async () =>
            {
                try { await _aiSync.SyncProductAsync(productDto); }
                catch (Exception ex) { Console.WriteLine($"[AI] Sync create thất bại: {ex.Message}"); }
            });

            return productDto;
            // return await GetByIdAsync(product.Id) ?? throw new Exception("Tạo thất bại");
        }

        // UPDATE – CHUẨN 100%, KHÔNG BAO GIỜ MẤT ẢNH BIẾN THỂ NỮA!
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

            // ẢNH CHÍNH
            if (dto.ImageUrls != null)
            {
                var removedImages = product.Images
                    .Where(img => img.ColorVariantId == null && !dto.ImageUrls.Contains(img.ImageUrl))
                    .ToList();
                foreach (var img in removedImages)
                {
                    _uploadService.DeleteFile(img.ImageUrl);
                    _context.ProductImages.Remove(img);
                }
            }

            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                var newUrls = await _uploadService.UploadProductImagesAsync(dto.ImageFiles);
                foreach (var url in newUrls)
                {
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = url,
                        ProductId = product.Id,
                        IsPrimary = false
                    });
                }
            }

            if (product.Images.Any(i => i.ColorVariantId == null))
            {
                foreach (var img in product.Images.Where(i => i.ColorVariantId == null)) img.IsPrimary = false;
                product.Images.First(i => i.ColorVariantId == null).IsPrimary = true;
            }

            // CHI TIẾT MÔ TẢ
            if (dto.Details != null)
            {
                var removed = product.Details.Where(d => !dto.Details.Any(x => x.Id == d.Id)).ToList();
                foreach (var d in removed)
                {
                    if (d.ImageUrl != null) _uploadService.DeleteFile(d.ImageUrl);
                    _context.ProductDetails.Remove(d);
                }

                foreach (var d in dto.Details)
                {
                    if (d.Id == 0)
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
                    else
                    {
                        var detail = product.Details.First(x => x.Id == d.Id);
                        detail.Text = d.Text;
                        detail.SortOrder = d.SortOrder;
                        if (d.ImageFile != null)
                        {
                            if (detail.ImageUrl != null) _uploadService.DeleteFile(detail.ImageUrl);
                            detail.ImageUrl = await _uploadService.UploadDetailImageAsync(d.ImageFile);
                        }
                    }
                }
            }

            // BIẾN THỂ MÀU – PHẦN QUAN TRỌNG NHẤT
            if (dto.ColorVariants != null)
            {
                // Xóa biến thể bị remove
                var removedVariants = product.ColorVariants
                    .Where(cv => !dto.ColorVariants.Any(x => x.Id == cv.Id))
                    .ToList();

                foreach (var cv in removedVariants)
                {
                    foreach (var img in cv.Images.ToList())
                    {
                        _uploadService.DeleteFile(img.ImageUrl);
                        _context.ProductImages.Remove(img);
                    }
                    _context.ProductSizeVariants.RemoveRange(cv.Sizes);
                    _context.ProductColorVariants.Remove(cv);
                }

                foreach (var cvDto in dto.ColorVariants)
                {
                    ProductColorVariant cv;

                    if (cvDto.Id == 0)
                    {
                        cv = new ProductColorVariant
                        {
                            Color = cvDto.Color,
                            ProductId = product.Id,
                            Images = new List<ProductImage>(),
                            Sizes = new List<ProductSizeVariant>()
                        };
                        product.ColorVariants.Add(cv);
                        _context.ProductColorVariants.Add(cv);
                        await _context.SaveChangesAsync(); // để có cv.Id
                    }
                    else
                    {
                        cv = product.ColorVariants.First(x => x.Id == cvDto.Id);
                        cv.Color = cvDto.Color;
                    }

                    // XỬ LÝ ẢNH BIẾN THỂ – CHUẨN 100%
                    if (cvDto.ImageUrls != null)
                    {
                        var imagesToRemove = cv.Images
                            .Where(img => !cvDto.ImageUrls.Contains(img.ImageUrl))
                            .ToList();

                        foreach (var img in imagesToRemove)
                        {
                            _uploadService.DeleteFile(img.ImageUrl);
                            _context.ProductImages.Remove(img);
                        }
                    }

                    // Thêm ảnh mới
                    if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                    {
                        var urls = await _uploadService.UploadVariantImagesAsync(cvDto.ImageFiles);
                        foreach (var url in urls)
                        {
                            cv.Images.Add(new ProductImage
                            {
                                ImageUrl = url,
                                ProductId = product.Id,
                                ColorVariantId = cv.Id,
                                IsPrimary = false
                            });
                        }
                    }

                    // Sizes
                    var removedSizes = cv.Sizes.Where(s => !cvDto.Sizes.Any(x => x.Id == s.Id)).ToList();
                    _context.ProductSizeVariants.RemoveRange(removedSizes);

                    foreach (var s in cvDto.Sizes)
                    {
                        if (s.Id == 0)
                        {
                            cv.Sizes.Add(new ProductSizeVariant
                            {
                                Size = s.Size,
                                Stock = s.Stock,
                                ColorVariantId = cv.Id
                            });
                        }
                        else
                        {
                            var size = cv.Sizes.First(x => x.Id == s.Id);
                            size.Size = s.Size;
                            size.Stock = s.Stock;
                        }
                    }
                }
            }

            product.Stock = product.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock);
            await _context.SaveChangesAsync();
            var updatedDto = await GetByIdAsync(product.Id);

            if (updatedDto != null)
            {
                _ = Task.Run(async () =>
                {
                    try { await _aiSync.SyncProductAsync(updatedDto); }
                    catch (Exception ex) { Console.WriteLine($"[AI] Sync update thất bại: {ex.Message}"); }
                });
            }

            return updatedDto;
          //  return await GetByIdAsync(product.Id);
        }

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
            _ = Task.Run(async () =>
            {
                try { await _aiSync.DeleteProductAsync(id); }
                catch (Exception ex) { Console.WriteLine($"[AI] Xóa chunk thất bại: {ex.Message}"); }
            });

            return true;
        }
    }
}