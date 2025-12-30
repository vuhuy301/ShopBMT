namespace backend_shopcaulong.DTOs.Cart
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int ColorVariantId { get; set; }
        public string Color { get; set; } = "";
        public int SizeVariantId { get; set; }
        public string Size { get; set; } = "";
        public int Quantity { get; set; }
    }
}
