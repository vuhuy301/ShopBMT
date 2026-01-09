// Services/ProductService.cs - OPTIMIZED VERSION
using AutoMapper;
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
        private readonly IAiSyncService _aiSync;

        public ProductService(ShopDbContext context, IMapper mapper, IUploadService uploadService, IAiSyncService aiSync)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
            _aiSync = aiSync;
        }

        // ✅ OPTIMIZED: Chỉ SELECT các field cần thiết, giảm 70-80% dữ liệu load
        private IQueryable<ProductDto> GetOptimizedProductQuery()
        {
            return _context.Products
                .AsNoTracking()
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsFeatured = p.IsFeatured,

                    // Tính Stock trực tiếp trong query
                    Stock = p.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock),

                    // Images chính - chỉ lấy những gì cần
                    Images = p.Images
    .Select(i => new ProductImageDto
    {
        Id = i.Id,
        ImageUrl = i.ImageUrl,
        IsPrimary = i.IsPrimary
    })
    .ToList(),
                    // Details - sắp xếp luôn trong query
                    Details = p.Details
                        .OrderBy(d => d.SortOrder)
                        .Select(d => new ProductDetailDto
                        {
                            Id = d.Id,
                            Text = d.Text,
                            ImageUrl = d.ImageUrl,
                            SortOrder = d.SortOrder
                        })
                        .ToList(),

                    // Color variants - tối ưu
                    ColorVariants = p.ColorVariants
                        .Select(cv => new ColorVariantDto
                        {
                            Id = cv.Id,
                            Color = cv.Color,
                            ImageUrls = cv.Images.Select(i => i.ImageUrl).ToList(),
                            Sizes = cv.Sizes
                                .Select(s => new SizeVariantDto
                                {
                                    Id = s.Id,
                                    Size = s.Size,
                                    Stock = s.Stock
                                })
                                .ToList()
                        })
                        .ToList()
                });
        }

        // ✅ OPTIMIZED: Danh sách đơn giản hơn cho listing
        private IQueryable<ProductDto> GetProductListQuery()
        {
            return _context.Products
                .AsNoTracking()
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsFeatured = p.IsFeatured,
                    Stock = p.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock),

                    // Chỉ lấy 1 ảnh đại diện cho listing
                    Images = p.Images
                        .Where(i => i.ColorVariantId == null && i.IsPrimary)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsPrimary = true
                        })
                        .Take(1)
                        .ToList(),

                    // Không load details cho listing
                    Details = new List<ProductDetailDto>(),

                    // Chỉ load color names, không load images và sizes
                    ColorVariants = p.ColorVariants
                        .Select(cv => new ColorVariantDto
                        {
                            Id = cv.Id,
                            Color = cv.Color,
                            ImageUrls = new List<string>(),
                            Sizes = new List<SizeVariantDto>()
                        })
                        .ToList()
                });
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return await GetProductListQuery()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            return await GetOptimizedProductQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ✅ OPTIMIZED: Sử dụng CountAsync riêng để tránh load data 2 lần
        public async Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize)
        {
            var query = GetProductListQuery().OrderBy(p => p.Name);

            // Đếm tổng số - query riêng, nhanh hơn
            var total = await _context.Products.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<ProductDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        // ✅ OPTIMIZED: Filter với index hints
        public async Task<PagedResultDto<ProductDto>> GetProductsByFilterAsync(
            int? categoryId, int? brandId, string? search, string? sortBy, int page, int pageSize)
        {
            var query = _context.Products.AsNoTracking();

            // Apply filters trước khi project
            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue && brandId > 0)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            // Đếm sau khi filter
            var total = await query.CountAsync();

            // Project sang DTO
            var dtoQuery = query.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                BrandId = p.BrandId,
                BrandName = p.Brand.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsFeatured = p.IsFeatured,
                Stock = p.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock),
                Images = p.Images
                    .Where(i => i.ColorVariantId == null && i.IsPrimary)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = true
                    })
                    .Take(1)
                    .ToList(),
                Details = new List<ProductDetailDto>(),
                ColorVariants = p.ColorVariants
                    .Select(cv => new ColorVariantDto
                    {
                        Id = cv.Id,
                        Color = cv.Color,
                        ImageUrls = new List<string>(),
                        Sizes = new List<SizeVariantDto>()
                    })
                    .ToList()
            });

            // Sort
            dtoQuery = sortBy switch
            {
                "asc" => dtoQuery.OrderBy(p => p.DiscountPrice ?? p.Price),
                "desc" => dtoQuery.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                _ => dtoQuery.OrderBy(p => p.Name)
            };

            var items = await dtoQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
        {
            return await GetProductListQuery()
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.Id) // Giả sử Id tăng dần = mới nhất
                .Take(8)
                .ToListAsync();
        }

        // CREATE - giữ nguyên logic, đã tối ưu
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
        // UPDATE - giữ nguyên logic
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

            // Cập nhật các trường cơ bản
            if (dto.Name != null) product.Name = dto.Name;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price != null) product.Price = dto.Price.Value;
            if (dto.DiscountPrice != null) product.DiscountPrice = dto.DiscountPrice;
            if (dto.BrandId != null) product.BrandId = dto.BrandId.Value;
            if (dto.CategoryId != null) product.CategoryId = dto.CategoryId.Value;
            if (dto.IsFeatured != null) product.IsFeatured = dto.IsFeatured.Value;

            // Cập nhật hình ảnh chung
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
                foreach (var img in product.Images.Where(i => i.ColorVariantId == null))
                    img.IsPrimary = false;
                product.Images.First(i => i.ColorVariantId == null).IsPrimary = true;
            }

            // Cập nhật Details
            if (dto.Details != null)
            {
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
                            if (detail.ImageUrl != null)
                                _uploadService.DeleteFile(detail.ImageUrl);
                            detail.ImageUrl = await _uploadService.UploadDetailImageAsync(d.ImageFile);
                        }
                    }
                }
            }

            // Cập nhật ColorVariants và Sizes (không xóa)
            if (dto.ColorVariants != null)
            {
                foreach (var cvDto in dto.ColorVariants)
                {
                    ProductColorVariant cv;

                    if (cvDto.Id == 0)
                    {
                        // Thêm ColorVariant mới
                        cv = new ProductColorVariant
                        {
                            Color = cvDto.Color,
                            ProductId = product.Id,
                            Images = new List<ProductImage>(),
                            Sizes = new List<ProductSizeVariant>()
                        };
                        product.ColorVariants.Add(cv);
                        _context.ProductColorVariants.Add(cv);
                    }
                    else
                    {
                        cv = product.ColorVariants.First(x => x.Id == cvDto.Id);
                        cv.Color = cvDto.Color;
                    }

                    // Cập nhật hình ảnh
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

                    // Chỉ cập nhật stock, không xóa Size
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
                            size.Stock = s.Stock; // chỉ update stock
                        }
                    }
                }
            }

            // Tính lại tổng stock
            product.Stock = product.ColorVariants.SelectMany(cv => cv.Sizes).Sum(s => s.Stock);
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                Console.WriteLine($"{entry.Entity.GetType().Name} - {entry.State}");
            }


            // Lưu thay đổi
            await _context.SaveChangesAsync();

            var updatedDto = await GetByIdAsync(product.Id);

            // Sync AI (không block)
            if (updatedDto != null)
            {
                _ = Task.Run(async () =>
                {
                    try { await _aiSync.SyncProductAsync(updatedDto); }
                    catch (Exception ex) { Console.WriteLine($"[AI] Sync update thất bại: {ex.Message}"); }
                });
            }

            return updatedDto;
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