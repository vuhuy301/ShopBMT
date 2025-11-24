namespace backend_shopcaulong.DTOs.Cart
{
    public class CheckoutRequestDto
    {
        // null hoặc rỗng => checkout tất cả item có Selected = true
        public List<int>? CartItemIds { get; set; }

        // Thông tin giao hàng / thanh toán
        public string ShippingAddress { get; set; }
        public string Phone { get; set; }
        public string PaymentMethod { get; set; } // "COD", "VNPay", ...
    }
}
