namespace backend_shopcaulong.DTOs.Order
{
    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? VariantId { get; set; }
        public string VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}