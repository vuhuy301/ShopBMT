namespace backend_shopcaulong.DTOs.Product
{
    public class ProductDetailCreateDto
{
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Thay vì string ImageUrl → nhận file
    public IFormFile? ImageFile { get; set; } // 1 ảnh cho 1 đoạn mô tả
}
}
