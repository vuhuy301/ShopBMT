namespace backend_shopcaulong.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }

        public string ProductName { get; set; }
        public string? VariantColor { get; set; }
        public string? VariantSize { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }  // price snapshot
        public bool Selected { get; set; }
    }
}
