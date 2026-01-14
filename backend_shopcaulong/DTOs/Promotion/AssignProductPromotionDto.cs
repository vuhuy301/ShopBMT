namespace backend_shopcaulong.DTOs.Promotion
{
    public class AssignProductPromotionDto
    {
        public int ProductId { get; set; }
        public List<int> PromotionIds { get; set; } = new();
    }
}
