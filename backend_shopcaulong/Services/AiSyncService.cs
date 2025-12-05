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
        var response = await _http.PostAsync($"{_url}/upsert_product", content);

        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"[AI] Sync thất bại: {await response.Content.ReadAsStringAsync()}");
    }

    public async Task DeleteProductAsync(int productId)
    {
        var json = JsonSerializer.Serialize(new { product_id = productId });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _http.PostAsync($"{_url}/delete_product", content);
        // Không throw lỗi → không làm hỏng xóa sản phẩm
    }

    public async Task RebuildAllAsync(List<ProductDto> products)
        {
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_url}/reindex_all", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AI] Rebuild thành công: {result}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AI] Rebuild thất bại: {response.StatusCode} - {error}");
            }
        }
}
}