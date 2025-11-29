using backend_shopcaulong.Models;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    // ⭐ Màu của sản phẩm (ColorVariant)
    public int ColorVariantId { get; set; }
    public ProductColorVariant ColorVariant { get; set; }

    // ⭐ Size nằm trong màu (SizeVariant)
    public int SizeVariantId { get; set; }
    public ProductSizeVariant SizeVariant { get; set; }

    // số lượng
    public int Quantity { get; set; }

    // Lưu giá tại thời điểm thêm vào giỏ
    public decimal Price { get; set; }

    public bool Selected { get; set; } = true;
}
