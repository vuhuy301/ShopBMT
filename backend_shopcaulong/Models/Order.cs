namespace backend_shopcaulong.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Người mua (nếu bạn có bảng User/Parent thì để khóa ngoại)
        public int? UserId { get; set; }
        public User? User { get; set; }
        // Tổng tiền đơn hàng
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        // Thời gian tạo đơn
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Trạng thái đơn hàng
        // Pending, Paid, Shipping, Completed, Cancelled
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; }  // COD, VNPay, Momo
        public string ShippingAddress { get; set; }
        public string Phone { get; set; }

        public string? Note { get; set; }

        // Danh sách sản phẩm trong đơn
        public ICollection<OrderDetail> Items { get; set; }
    }
}
