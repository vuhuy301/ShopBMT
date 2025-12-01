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
            // 0. VALIDATION: bắt buộc có ít nhất 1 biến thể
            if (dto.ColorVariants == null || !dto.ColorVariants.Any())
                throw new ArgumentException("Sản phẩm phải có ít nhất 1 biến thể màu và size.");

            foreach (var cv in dto.ColorVariants)
            {
                if (cv.Sizes == null || !cv.Sizes.Any())
                    throw new ArgumentException($"Biến thể màu '{cv.Color}' phải có ít nhất 1 size.");
            }

            // 1. Tạo product trước để lấy ID thật
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
            await _context.SaveChangesAsync();  // ← quan trọng để có product.Id thật

            // 2. Upload ảnh chính của product
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

            // 3. Chi tiết mô tả
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

            // 4. Xử lý biến thể màu
            foreach (var cvDto in dto.ColorVariants!)
            {
                var colorVariant = new ProductColorVariant
                {
                    Color = cvDto.Color,
                    Images = new List<ProductImage>(),
                    Sizes = new List<ProductSizeVariant>(),
                    ProductId = product.Id
                };

                // Ảnh cho biến thể màu
                if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                {
                    var urls = await _uploadService.UploadProductImagesAsync(cvDto.ImageFiles);

                    colorVariant.Images = urls.Select((url, i) => new ProductImage
                    {
                        ImageUrl = url,
                        IsPrimary = i == 0,
                        ProductId = product.Id
                    }).ToList();
                }

                // Size variants
                foreach (var s in cvDto.Sizes)
                {
                    colorVariant.Sizes.Add(new ProductSizeVariant
                    {
                        Size = s.Size,
                        Stock = s.Stock
                    });
                }

                product.ColorVariants.Add(colorVariant);
            }

            // 5. Tính tổng stock = tổng stock mọi size
            product.Stock = product.ColorVariants
                .SelectMany(c => c.Sizes)
                .Sum(s => s.Stock);

            // 6. Lưu DB
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

            // ====================================================
            // 1) UPDATE BASIC INFORMATION
            // ====================================================
            if (dto.Name != null) product.Name = dto.Name;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price != null) product.Price = dto.Price.Value;
            if (dto.DiscountPrice != null) product.DiscountPrice = dto.DiscountPrice;
            if (dto.BrandId != null) product.BrandId = dto.BrandId.Value;
            if (dto.CategoryId != null) product.CategoryId = dto.CategoryId.Value;
            if (dto.IsFeatured != null) product.IsFeatured = dto.IsFeatured.Value;

            // ====================================================
            // 2) MAIN PRODUCT IMAGES
            // ====================================================

            // XÓA ảnh cũ mà frontend KHÔNG gửi nữa
            if (dto.ImageUrls != null)
            {
                var removedImages = product.Images
                    .Where(img => !dto.ImageUrls.Contains(img.ImageUrl))
                    .ToList();

                foreach (var img in removedImages)
                {
                    _uploadService.DeleteFile(img.ImageUrl);
                    _context.ProductImages.Remove(img);
                }
            }

            // Thêm ảnh mới
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

            // Set ảnh đầu tiên làm primary
            if (product.Images.Any())
            {
                foreach (var img in product.Images) img.IsPrimary = false;
                product.Images.First().IsPrimary = true;
            }

            // ====================================================
            // 3) PRODUCT DETAILS
            // ====================================================
            if (dto.Details != null)
            {
                // XÓA detail không còn
                var removed = product.Details
                    .Where(d => !dto.Details.Any(x => x.Id == d.Id))
                    .ToList();

                foreach (var d in removed)
                {
                    if (d.ImageUrl != null)
                        _uploadService.DeleteFile(d.ImageUrl);

                    _context.ProductDetails.Remove(d);
                }

                // UPDATE + ADD
                foreach (var d in dto.Details)
                {
                    ProductDetail detail;

                    if (d.Id == 0)
                    {
                        // NEW DETAIL
                        string? url = null;
                        if (d.ImageFile != null)
                            url = await _uploadService.UploadDetailImageAsync(d.ImageFile);

                        detail = new ProductDetail
                        {
                            Text = d.Text,
                            SortOrder = d.SortOrder,
                            ImageUrl = url,
                            ProductId = product.Id
                        };

                        product.Details.Add(detail);
                    }
                    else
                    {
                        // UPDATE EXISTING DETAIL
                        detail = product.Details.First(x => x.Id == d.Id);

                        detail.Text = d.Text;
                        detail.SortOrder = d.SortOrder;

                        // Nếu có ảnh mới → xoá ảnh cũ + upload mới
                        if (d.ImageFile != null)
                        {
                            if (detail.ImageUrl != null)
                                _uploadService.DeleteFile(detail.ImageUrl);

                            detail.ImageUrl = await _uploadService.UploadDetailImageAsync(d.ImageFile);
                        }
                        else
                        {
                            // Giữ ảnh cũ nếu không upload ảnh mới
                            detail.ImageUrl = d.ImageUrl;
                        }
                    }
                }
            }

            // ====================================================
            // 4) COLOR VARIANTS UPDATE
            // ====================================================
            if (dto.ColorVariants != null)
            {
                // XÓA color variant bị remove
                var removedCv = product.ColorVariants
                    .Where(cv => !dto.ColorVariants.Any(x => x.Id == cv.Id))
                    .ToList();

                foreach (var cv in removedCv)
                {
                    foreach (var img in cv.Images)
                        _uploadService.DeleteFile(img.ImageUrl);

                    _context.ProductImages.RemoveRange(cv.Images);
                    _context.ProductSizeVariants.RemoveRange(cv.Sizes);
                    _context.ProductColorVariants.Remove(cv);
                }

                // UPDATE or ADD
                foreach (var cvDto in dto.ColorVariants)
                {
                    ProductColorVariant cv;

                    if (cvDto.Id == 0)
                    {
                        // ADD NEW COLOR VARIANT
                        cv = new ProductColorVariant
                        {
                            Color = cvDto.Color,
                            ProductId = product.Id,
                            Images = new List<ProductImage>(),
                            Sizes = new List<ProductSizeVariant>()
                        };

                        product.ColorVariants.Add(cv);
                    }
                    else
                    {
                        // UPDATE EXISTING
                        cv = product.ColorVariants.First(x => x.Id == cvDto.Id);
                        cv.Color = cvDto.Color;
                    }

                    // ===================== IMAGES =====================
                    if (cvDto.ImageUrls != null)
                    {
                        var removedImgs = cv.Images
                            .Where(img => !cvDto.ImageUrls.Contains(img.ImageUrl))
                            .ToList();

                        foreach (var img in removedImgs)
                        {
                            _uploadService.DeleteFile(img.ImageUrl);
                            _context.ProductImages.Remove(img);
                        }
                    }

                    // Thêm ảnh mới
                    if (cvDto.ImageFiles != null && cvDto.ImageFiles.Count > 0)
                    {
                        var newUrls = await _uploadService.UploadProductImagesAsync(cvDto.ImageFiles);
                        foreach (var url in newUrls)
                        {
                            cv.Images.Add(new ProductImage
                            {
                                ImageUrl = url,
                                ProductId = product.Id
                            });
                        }
                    }

                    // ===================== SIZES =====================
                    var removedSizes = cv.Sizes
                        .Where(s => !cvDto.Sizes.Any(x => x.Id == s.Id))
                        .ToList();

                    _context.ProductSizeVariants.RemoveRange(removedSizes);

                    foreach (var s in cvDto.Sizes)
                    {
                        ProductSizeVariant size;

                        if (s.Id == 0)
                        {
                            // NEW SIZE
                            size = new ProductSizeVariant
                            {
                                Size = s.Size,
                                Stock = s.Stock,
                                ColorVariantId = cv.Id
                            };
                            cv.Sizes.Add(size);
                        }
                        else
                        {
                            // UPDATE SIZE
                            size = cv.Sizes.First(x => x.Id == s.Id);
                            size.Size = s.Size;
                            size.Stock = s.Stock;
                        }
                    }
                }
            }

            // ====================================================
            // 5) UPDATE TỔNG STOCK
            // ====================================================
            product.Stock = product.ColorVariants
                .SelectMany(cv => cv.Sizes)
                .Sum(s => s.Stock);

            // SAVE
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
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
            return true;
        }
    }
}