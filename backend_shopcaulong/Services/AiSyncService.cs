using System.Text;
using System.Text.Json;
using backend_shopcaulong.DTOs.Product;

namespace backend_shopcaulong.Services
{
    
    public class AiSyncService : IAiSyncService
{
    private readonly HttpClient _http;
    private readonly string _url = "http://localhost:8000"; // deploy thì đổi

    public AiSyncService(HttpClient http) => _http = http;

        public async Task SyncProductAsync(ProductDto product)
        {
            var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _http.PostAsync("/update_product", content); // ĐÃ SỬA

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AI] Sync thất bại: {response.StatusCode} - {error}");
            }
            else
            {
                Console.WriteLine($"[AI] Sync thành công sản phẩm: {product.Name}");
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            var json = JsonSerializer.Serialize(new { product_id = productId });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/delete_product", content); // ĐÃ SỬA

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"[AI] Xóa chunk thất bại ID: {productId}");
        }

        public async Task RebuildAllAsync(List<ProductDto> products)
        {
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _http.PostAsync("/reindex_all", content); // ĐÃ SỬA

            if (response.IsSuccessStatusCode)
                Console.WriteLine("[AI] Rebuild toàn bộ thành công!");
            else
                Console.WriteLine($"[AI] Rebuild thất bại: {await response.Content.ReadAsStringAsync()}");
        }
}
}