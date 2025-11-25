using Microsoft.EntityFrameworkCore;
using backend_shopcaulong.Models;
using backend_shopcaulong.DTOs.Role;

namespace backend_shopcaulong.Services{
    
    public class RoleService : IRoleService
    {
        private readonly ShopDbContext _context;

        public RoleService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetAllAsync()
        {
            return await _context.Roles
                .Select(r => new RoleDto { Id = r.Id, Name = r.Name })
                .ToListAsync();
        }

        public async Task<RoleDto?> GetByIdAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;
            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        public async Task<RoleDto> CreateAsync(RoleCreateDto dto)
        {
            var role = new Role { Name = dto.Name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        public async Task<RoleDto?> UpdateAsync(int id, RoleUpdateDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;

            role.Name = dto.Name;
            await _context.SaveChangesAsync();

            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}