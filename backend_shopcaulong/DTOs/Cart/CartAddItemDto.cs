namespace backend_shopcaulong.DTOs.Cart
{
    public class CartAddItemDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
