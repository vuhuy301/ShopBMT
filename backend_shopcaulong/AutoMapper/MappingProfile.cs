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
    // Profiles/MappingProfile.cs
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product → ProductDto
            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            // ProductCreateDto → Product (chỉ map các field cơ bản)
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            // ProductUpdateDto → Product (tương tự)
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            // Các collection con
            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductDetail, ProductDetailDto>();
            CreateMap<ProductVariant, ProductVariantDto>();

            // Category
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

            CreateMap<CategoryCreateUpdateDto, Category>();
            CreateMap<Brand, BrandDto>()
    .ForMember(dest => dest.ProductCount,
               opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<BrandCreateDto, Brand>();
            CreateMap<BrandUpdateDto, Brand>();

            CreateMap<Cart, CartDto>()
             .ForMember(d => d.CartId, opt => opt.MapFrom(s => s.Id))
             .ForMember(d => d.TotalAmount,
                 opt => opt.MapFrom(s => s.Items.Sum(i => i.Price * i.Quantity)))
             .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));


            // CartItem → CartItemDto
            CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.VariantColor, opt => opt.MapFrom(s => s.Variant.Color))
            .ForMember(d => d.VariantSize, opt => opt.MapFrom(s => s.Variant.Size));

             CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.UserFullName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Khách vãng lai"))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity))) // Tính chính xác 100%
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.VariantName,
                    opt => opt.MapFrom(src => src.Variant != null
                        ? $"{src.Variant.Color} {src.Variant.Size}".Trim()
                        : string.Empty));
        }
    }
}
