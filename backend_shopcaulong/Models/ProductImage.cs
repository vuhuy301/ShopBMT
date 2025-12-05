namespace backend_shopcaulong.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; } = false;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // THÊM 2 DÒNG NÀY – BẮT BUỘC
        public int? ColorVariantId { get; set; }
        public ProductColorVariant? ColorVariant { get; set; }
    }
}