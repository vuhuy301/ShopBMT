namespace backend_shopcaulong.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // FK → User (bắt buộc)
        public int UserId { get; set; }
        public User User { get; set; }   // ← Navigation property

        // Items trong cart
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Để phân biệt cart hiện tại hay cart đã checkout
        public string Status { get; set; } = "Active"; // Active / CheckedOut

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
