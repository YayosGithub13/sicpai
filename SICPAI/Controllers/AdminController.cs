using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using SICPAI.Security;

namespace SICPAI.Controllers
{
    [Authorize(Roles = "Administrator")]
    [PermissionAuthorize("users.manage")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Usuarios()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return View();
        }

        public IActionResult Bitacora()
        {
            return View();
        }

        public IActionResult Configuracion()
        {
            return View();
        }

        [HttpGet("/api/admin/usuarios")]
        public async Task<IActionResult> ListUsers()
        {
            var users = await _db.Users.AsNoTracking().Include(u => u.UserRoles).ThenInclude(ur => ur.RoleCatalog).OrderBy(u => u.Name).Select(u => new { u.Id, u.UserName, u.Email, u.Name, u.LastName, u.ActiveFlag, Roles = u.UserRoles.Where(ur => ur.ActiveFlag).Select(ur => ur.RoleCatalog.Name) }).ToListAsync();
            return Ok(users);
        }

        [HttpPost("/api/admin/usuarios")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { error = "Usuario, correo y contraseña son obligatorios." });
            if (await _db.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email)) return Conflict(new { error = "El usuario o correo ya existe." });
            var user = new User(request.UserName.Trim(), request.Email.Trim(), string.Empty) { Name = request.Name.Trim(), LastName = request.LastName.Trim() };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            if (request.RoleId.HasValue) await SetRole(user.Id, request.RoleId.Value);
            await Log("Crear", nameof(User), user.Id.ToString(), $"Usuario {user.UserName} creado.");
            await _db.SaveChangesAsync();
            return Created($"/api/admin/usuarios/{user.Id}", new { user.Id, user.UserName, user.Email });
        }

        [HttpPatch("/api/admin/usuarios/{id:int}/estado")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatusRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.ActiveFlag = request.Active;
            user.LastUpdate = DateTime.UtcNow;
            await Log("Actualizar estado", nameof(User), id.ToString(), $"Usuario {user.UserName}: {(request.Active ? "activado" : "desactivado")}.");
            await _db.SaveChangesAsync();
            return Ok(new { user.Id, user.ActiveFlag });
        }

        [HttpGet("/api/admin/roles")]
        public async Task<IActionResult> ListRoles() => Ok(await _db.RoleCatalogs.AsNoTracking().Include(r => r.RolePermissions).ThenInclude(rp => rp.PermissionCatalog).OrderBy(r => r.Name).ToListAsync());

        [HttpPost("/api/admin/roles")]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "El nombre del rol es obligatorio." });
            if (await _db.RoleCatalogs.AnyAsync(r => r.Name == request.Name)) return Conflict(new { error = "El rol ya existe." });
            var role = new RoleCatalog { Name = request.Name.Trim(), Description = request.Description?.Trim(), ActiveFlag = true, LastUpdate = DateTime.UtcNow };
            _db.RoleCatalogs.Add(role); await _db.SaveChangesAsync(); return Created($"/api/admin/roles/{role.Id}", role);
        }

        [HttpPut("/api/admin/roles/{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequest request)
        {
            var role = await _db.RoleCatalogs.FindAsync(id); if (role == null) return NotFound();
            role.Name = request.Name.Trim(); role.Description = request.Description?.Trim(); role.ActiveFlag = request.Active; role.LastUpdate = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return Ok(role);
        }

        [HttpGet("/api/admin/permisos")]
        public async Task<IActionResult> ListPermissions() => Ok(await _db.PermissionCatalogs.AsNoTracking().Where(p => p.Active).OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync());

        [HttpPut("/api/admin/roles/{roleId:int}/permisos")]
        public async Task<IActionResult> SetRolePermissions(int roleId, [FromBody] PermissionRequest request)
        {
            if (!await _db.RoleCatalogs.AnyAsync(r => r.Id == roleId)) return NotFound(new { error = "Rol no encontrado." });
            var validIds = await _db.PermissionCatalogs.Where(p => request.PermissionIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
            var current = await _db.RolePermissions.Where(rp => rp.RoleCatalogId == roleId).ToListAsync();
            _db.RolePermissions.RemoveRange(current);
            _db.RolePermissions.AddRange(validIds.Select(id => new RolePermission { RoleCatalogId = roleId, PermissionCatalogId = id }));
            await Log("Actualizar permisos", nameof(RoleCatalog), roleId.ToString(), $"Se asignaron {validIds.Count} permisos.");
            await _db.SaveChangesAsync();
            return Ok(new { roleId, permissions = validIds });
        }

        private async Task SetRole(int userId, int roleId)
        {
            if (await _db.RoleCatalogs.AnyAsync(r => r.Id == roleId)) _db.UserRoles.Add(new UserRole { UserId = userId, RoleCatalogId = roleId, ActiveFlag = true, LastUpdate = DateTime.UtcNow });
        }

        private Task Log(string action, string entity, string id, string description)
        {
            _db.ActivityLogs.Add(new ActivityLog { UserId = CurrentUserId(), Action = action, EntityName = entity, EntityId = id, Description = description });
            return Task.CompletedTask;
        }
        private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public sealed class UserRequest { public string UserName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string LastName { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public int? RoleId { get; set; } }
        public sealed class UserStatusRequest { public bool Active { get; set; } }
        public sealed class PermissionRequest { public int[] PermissionIds { get; set; } = Array.Empty<int>(); }
        public sealed class RoleRequest { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public bool Active { get; set; } = true; }
    }
}
