
// // Controllers/RoleController.cs
// using Microsoft.AspNetCore.Mvc;
// using backend_shopcaulong.DTOs.Role;
// using backend_shopcaulong.Models;
// using backend_shopcaulong.Services;
// using Microsoft.AspNetCore.Http;

// namespace backend_shopcaulong.Controllers {

// [ApiController]
// [Route("api/[controller]")]
// public class RolesController : ControllerBase
// {
//     private readonly IRoleService _roleService;

//     public RolesController(IRoleService roleService)
//     {
//         _roleService = roleService;
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         var roles = await _roleService.GetAllAsync();
//         return Ok(roles);
//     }

//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetById(int id)
//     {
//         var role = await _roleService.GetByIdAsync(id);
//         if (role == null) return NotFound();
//         return Ok(role);
//     }

//     [HttpPost]
//     public async Task<IActionResult> Create(RoleCreateDto dto)
//     {
//         var role = await _roleService.CreateAsync(dto);
//         return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
//     }

//     [HttpPut("{id}")]
//     public async Task<IActionResult> Update(int id, RoleUpdateDto dto)
//     {
//         var updatedRole = await _roleService.UpdateAsync(id, dto);
//         if (updatedRole == null) return NotFound();
//         return Ok(updatedRole);
//     }

//     [HttpDelete("{id}")]
//     public async Task<IActionResult> Delete(int id)
//     {
//         var success = await _roleService.DeleteAsync(id);
//         if (!success) return NotFound();
//         return NoContent();
//     }
// }
   
// }