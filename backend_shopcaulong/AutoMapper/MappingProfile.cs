using AutoMapper;
using backend_shopcaulong.DTOs.Brand;
using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.DTOs.Product;
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

        }
    }
}
