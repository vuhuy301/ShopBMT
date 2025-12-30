namespace backend_shopcaulong.DTOs.Cart
{
    public class StockCheckItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Requested { get; set; }
        public int Available { get; set; }
        public string Message { get; set; }
    }
}
