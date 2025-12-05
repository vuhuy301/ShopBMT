using backend_shopcaulong.DTOs.Product;

namespace backend_shopcaulong.Services
{
    
    public interface IAiSyncService
{
    Task SyncProductAsync(ProductDto product);
    Task DeleteProductAsync(int productId);

    Task RebuildAllAsync(List<ProductDto> products);
}
}