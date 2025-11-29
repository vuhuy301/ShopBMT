using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.DTOs.Product
{
   public class ProductUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int? BrandId { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsFeatured { get; set; }

    // Nếu có gửi ảnh mới → thay thế toàn bộ
    public IFormFileCollection? ImageFiles { get; set; }

    public List<ProductDetailCreateDto>? Details { get; set; }
    public List<ColorVariantDto> ColorVariants { get; set; } = new();
    }
}
