using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Application.Interfaces;
using SICPAI.Models;
using Infrastructure.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace SICPAI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(IUserService userService, AppDbContext db)
        {
            _userService = userService;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.ValidateUserAsync(model.Email ?? string.Empty, model.Password ?? string.Empty);
            if (user == null || !user.ActiveFlag)
            {
                _db.ActivityLogs.Add(new ActivityLog { Action = "Login fallido", EntityName = nameof(User), Description = $"Intento de acceso para {model.Email}.", IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "El usuario o la contraseña no son válidos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.Name} {user.LastName}".Trim()),
                new(ClaimTypes.Email, user.Email)
            };
            claims.AddRange(user.UserRoles.Where(ur => ur.ActiveFlag && ur.RoleCatalog.ActiveFlag)
                .Select(ur => new Claim(ClaimTypes.Role, ur.RoleCatalog.Name)));
            claims.AddRange(user.UserRoles.Where(ur => ur.ActiveFlag)
                .SelectMany(ur => ur.RoleCatalog.RolePermissions.Where(rp => rp.Active && rp.PermissionCatalog.Active))
                .Select(rp => new Claim("permission", rp.PermissionCatalog.Code)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = model.RememberMe });
            _db.ActivityLogs.Add(new ActivityLog { UserId = user.Id, Action = "Login exitoso", EntityName = nameof(User), EntityId = user.Id.ToString(), IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
            await _db.SaveChangesAsync();

            return LocalRedirect(model.ReturnUrl ?? "/Home/Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Logout", EntityName = nameof(User), EntityId = userId.ToString(), IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (model.NewPassword.Length < 12 || !model.NewPassword.Any(char.IsUpper) || !model.NewPassword.Any(char.IsLower) || !model.NewPassword.Any(char.IsDigit))
                ModelState.AddModelError(nameof(model.NewPassword), "La contraseña debe tener al menos 12 caracteres, mayúsculas, minúsculas y números.");
            if (model.NewPassword != model.ConfirmPassword) ModelState.AddModelError(nameof(model.ConfirmPassword), "Las contraseñas no coinciden.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
            var user = await _db.Users.FindAsync(userId);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed) return BadRequest(new { error = "La contraseña actual no es válida." });
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword); user.LastUpdate = DateTime.UtcNow;
            _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = "Cambio de contraseña", EntityName = nameof(User), EntityId = userId.ToString() });
            await _db.SaveChangesAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Contraseña actualizada. Inicie sesión nuevamente." });
        }

        public IActionResult AccesoDenegado() => View();

        public IActionResult Index() => RedirectToAction(nameof(Login));
    }
}
