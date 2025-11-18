namespace backend_shopcaulong.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tên ưu đãi, ví dụ "Miễn phí vận chuyển"
        public string Description { get; set; } // Mô tả chi tiết

        // Navigation property
        public ICollection<ProductPromotion> ProductPromotions { get; set; }
    }
}
