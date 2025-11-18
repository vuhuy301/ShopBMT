namespace backend_shopcaulong.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Văn bản mô tả
        public string? Text { get; set; }

        // Ảnh mô tả
        public string? ImageUrl { get; set; }

        // Thứ tự hiển thị (block 1, block 2, block 3, …)
        public int SortOrder { get; set; }
    }
}
