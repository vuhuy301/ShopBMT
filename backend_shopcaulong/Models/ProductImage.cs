namespace backend_shopcaulong.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        // Là ảnh chính hay ảnh phụ
        public bool IsPrimary { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
