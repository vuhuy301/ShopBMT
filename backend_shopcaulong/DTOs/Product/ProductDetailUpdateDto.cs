namespace backend_shopcaulong.DTOs.Product
{
    public class ProductDetailUpdateDto
    {
        public int Id { get; set; } // 0 nếu thêm mới
        public string Text { get; set; }
        public int SortOrder { get; set; }

        public string? ImageUrl { get; set; } // Giữ ảnh cũ
        public IFormFile? ImageFile { get; set; } // Thêm ảnh mới
    }


}
