using backend_shopcaulong.DTOs.Role;

namespace backend_shopcaulong.Services{
    public interface IRoleService
        {
            Task<List<RoleDto>> GetAllAsync();
            Task<RoleDto?> GetByIdAsync(int id);
            Task<RoleDto> CreateAsync(RoleCreateDto dto);
            Task<RoleDto?> UpdateAsync(int id, RoleUpdateDto dto);
            Task<bool> DeleteAsync(int id);
        }
}