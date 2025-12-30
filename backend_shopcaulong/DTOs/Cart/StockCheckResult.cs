namespace backend_shopcaulong.DTOs.Cart
{
    public class StockCheckResult
    {
        public bool Ok { get; set; } = true;
        public List<StockCheckItem> Items { get; set; } = new();
    }
}
