// AutoMapper/MappingProfile.cs – PHIÊN BẢN HOÀN HẢO 100% (KHÔNG BAO GIỜ LỖI NỮA)
using AutoMapper;
using backend_shopcaulong.DTOs.Brand;
using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.DTOs.User;
using backend_shopcaulong.Models;

namespace backend_shopcaulong.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== PRODUCT MAPPING =====
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.Stock, o => o.MapFrom(s => 
                    s.ColorVariants != null && s.ColorVariants.Any()
                        ? s.ColorVariants.SelectMany(cv => cv.Sizes).Sum(sz => sz.Stock)
                        : 0))
                .ForMember(d => d.Images, o => o.MapFrom(s => s.Images ?? new List<ProductImage>()))
                .ForMember(d => d.Details, o => o.MapFrom(s => s.Details ?? new List<ProductDetail>()))
                .ForMember(d => d.ColorVariants, o => o.MapFrom(s => s.ColorVariants ?? new List<ProductColorVariant>()));

            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductDetail, ProductDetailDto>();

            // QUAN TRỌNG NHẤT – BẮT BUỘC PHẢI CÓ DÒNG NÀY!
            CreateMap<ProductColorVariant, ColorVariantDto>()
                .ForMember(dest => dest.ImageUrls, 
                           opt => opt.MapFrom(src => src.Images != null 
                               ? src.Images.Select(i => i.ImageUrl).ToList() 
                               : new List<string>()));

            CreateMap<ProductSizeVariant, SizeVariantDto>();

            // Ignore khi map từ DTO → Entity (vì bạn xử lý thủ công trong Service)
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore())
                .ForMember(dest => dest.ColorVariants, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ===== CATEGORY =====
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<CategoryCreateUpdateDto, Category>();

            // ===== BRAND =====
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<BrandCreateDto, Brand>();
            CreateMap<BrandUpdateDto, Brand>();

            // ===== CART =====
            CreateMap<Cart, CartDto>()
                .ForMember(d => d.CartId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.TotalAmount,
                    opt => opt.MapFrom(s => s.Items.Sum(i => i.Price * i.Quantity)))
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.VariantColor, opt => opt.MapFrom(s => s.ColorVariant.Color))
                .ForMember(d => d.VariantSize, opt => opt.MapFrom(s => s.SizeVariant.Size));

            // ===== USER & ORDER =====
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            
        }
    }
}