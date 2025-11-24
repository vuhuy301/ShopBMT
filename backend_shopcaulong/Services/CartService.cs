using AutoMapper;
using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.Models;
using backend_shopcaulong.Services;
using Microsoft.EntityFrameworkCore;
using System;

public class CartService : ICartService
{
    private readonly ShopDbContext _db;
    private readonly IMapper _mapper;

    public CartService(ShopDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // Lấy cart theo user
    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return _mapper.Map<CartDto>(cart);
    }

    // Thêm item
    public async Task AddItemAsync(int userId, CartAddItemDto dto)
    {
        var cart = await GetOrCreateCart(userId);

        // Tìm xem item đã có trong cart chưa
        var existing = cart.Items.FirstOrDefault(i =>
            i.ProductId == dto.ProductId &&
            i.VariantId == dto.VariantId
        );

        decimal price = await GetCurrentPrice(dto.ProductId, dto.VariantId);

        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = dto.ProductId,
                VariantId = dto.VariantId,
                Quantity = dto.Quantity,
                Price = price
            });
        }

        await _db.SaveChangesAsync();
    }

    // Update quantity
    public async Task UpdateItemAsync(int userId, CartUpdateItemDto dto)
    {
        var cart = await GetOrCreateCart(userId);

        var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId);
        if (item == null) throw new Exception("Item not found");

        item.Quantity = dto.Quantity;

        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int userId, int cartItemId)
    {
        var cart = await GetOrCreateCart(userId);

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) throw new Exception("Item not found");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ToggleSelectAsync(int userId, int cartItemId)
    {
        var cart = await GetOrCreateCart(userId);

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) throw new Exception("Item not found");

        item.Selected = !item.Selected;
        await _db.SaveChangesAsync();
    }

    // CHECKOUT
    public async Task<OrderResultDto> CheckoutAsync(int userId, CheckoutRequestDto dto)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

        if (cart == null || !cart.Items.Any())
            throw new Exception("Cart empty");

        // Lấy danh sách item sẽ mua
        var itemsToBuy = (dto.CartItemIds == null || !dto.CartItemIds.Any())
            ? cart.Items.Where(i => i.Selected).ToList()
            : cart.Items.Where(i => dto.CartItemIds.Contains(i.Id)).ToList();

        if (!itemsToBuy.Any())
            throw new Exception("No items selected to checkout");

        // Trừ tồn kho
        foreach (var item in itemsToBuy)
        {
            if (item.VariantId != null)
            {
                item.Variant.Stock -= item.Quantity;
            }
            else
            {
                item.Product.Stock -= item.Quantity;
            }
        }

        // Tạo Order
        var order = new Order
        {
            UserId = userId,
            ShippingAddress = dto.ShippingAddress,
            Phone = dto.Phone,
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Order details
        foreach (var item in itemsToBuy)
        {
            _db.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        // Xóa item đã mua
        _db.CartItems.RemoveRange(itemsToBuy);

        // Nếu cart hết item → giữ vẫn Active hoặc làm rỗng
        await _db.SaveChangesAsync();

        return new OrderResultDto
        {
            OrderId = order.Id,
            TotalAmount = itemsToBuy.Sum(i => i.Price * i.Quantity),
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }

    private async Task<Cart> GetOrCreateCart(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<decimal> GetCurrentPrice(int productId, int? variantId)
    {
        if (variantId != null)
        {
            var variant = await _db.ProductVariants.FindAsync(variantId);
            if (variant == null)
                throw new InvalidOperationException("Variant not found.");

            // Nếu Price null thì dùng giá Product
            if (variant.Price.HasValue)
                return variant.Price.Value;

            // Nếu không có giá riêng, lấy giá chung của sản phẩm
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found.");

            return product.Price;
        }
        else
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found.");

            return product.Price;
        }
    }

}
