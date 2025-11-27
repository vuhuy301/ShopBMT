using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Models;

namespace backend_shopcaulong.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(ProductCreateDto dto);
        Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize);
        Task<PagedResultDto<ProductDto>> GetProductsByFilterAsync(int? categoryId, int? brandId, string? search, string? sortBy, int page, int pageSize);
        Task<List<ProductDto>> GetTopNewByCategoryAsync(int categoryId);
    }
}
