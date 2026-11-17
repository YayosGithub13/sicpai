using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SICPAI.Security;
using Application.Interfaces;

namespace SICPAI.Controllers
{
    [Authorize(Roles = "Administrator")]
    [PermissionAuthorize("users.manage")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService) => _adminService = adminService;

        public IActionResult Usuarios() => View();
        public IActionResult Roles() => View();
        public IActionResult Bitacora() => View();
        public IActionResult Configuracion() => View();

        [HttpGet("/api/admin/usuarios")]
        public async Task<IActionResult> ListUsers() => Ok(await _adminService.ListUsersAsync(HttpContext.RequestAborted));

        [HttpPost("/api/admin/usuarios")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { error = "Usuario, correo y contraseña son obligatorios." });
            try
            {
                var user = await _adminService.CreateUserAsync(request.UserName, request.Email, request.Name, request.LastName, request.Password, request.RoleId, CurrentUserId(), HttpContext.RequestAborted);
                return Created($"/api/admin/usuarios/{user!.Id}", new { user.Id, user.UserName, user.Email });
            }
            catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        }

        [HttpPatch("/api/admin/usuarios/{id:int}/estado")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatusRequest request)
        {
            var user = await _adminService.UpdateUserStatusAsync(id, request.Active, CurrentUserId(), HttpContext.RequestAborted);
            return user is null ? NotFound() : Ok(new { user.Id, user.ActiveFlag });
        }

        [HttpGet("/api/admin/roles")]
        public async Task<IActionResult> ListRoles() => Ok(await _adminService.ListRolesAsync(HttpContext.RequestAborted));

        [HttpPost("/api/admin/roles")]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "El nombre del rol es obligatorio." });
            try { return Created($"/api/admin/roles/0", await _adminService.CreateRoleAsync(request.Name, request.Description, HttpContext.RequestAborted)); }
            catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        }

        [HttpPut("/api/admin/roles/{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequest request)
        {
            var role = await _adminService.UpdateRoleAsync(id, request.Name, request.Description, request.Active, HttpContext.RequestAborted);
            return role is null ? NotFound() : Ok(role);
        }

        [HttpGet("/api/admin/permisos")]
        public async Task<IActionResult> ListPermissions() => Ok(await _adminService.ListPermissionsAsync(HttpContext.RequestAborted));

        [HttpPut("/api/admin/roles/{roleId:int}/permisos")]
        public async Task<IActionResult> SetRolePermissions(int roleId, [FromBody] PermissionRequest request)
        {
            var permissions = await _adminService.SetRolePermissionsAsync(roleId, request.PermissionIds, CurrentUserId(), HttpContext.RequestAborted);
            return permissions is null ? NotFound(new { error = "Rol no encontrado." }) : Ok(new { roleId, permissions });
        }

        private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        public sealed class UserRequest { public string UserName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string LastName { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public int? RoleId { get; set; } }
        public sealed class UserStatusRequest { public bool Active { get; set; } }
        public sealed class PermissionRequest { public int[] PermissionIds { get; set; } = Array.Empty<int>(); }
        public sealed class RoleRequest { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public bool Active { get; set; } = true; }
    }
}
