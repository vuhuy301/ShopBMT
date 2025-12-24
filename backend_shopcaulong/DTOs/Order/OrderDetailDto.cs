namespace backend_shopcaulong.DTOs.Order
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Lấy từ Product.Name
        public int ColorVariantId { get; set; }
        public string Color { get; set; } = string.Empty; // Lấy từ ColorVariant.Color
        public int SizeVariantId { get; set; }
        public string Size { get; set; } = string.Empty; // Lấy từ SizeVariant.Size
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;

        public string? ImageUrl { get; set; }
    }
}