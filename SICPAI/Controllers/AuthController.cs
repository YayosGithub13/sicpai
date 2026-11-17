using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecurityClaim = System.Security.Claims.Claim;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Application.Interfaces;
using SICPAI.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using ApplicationAuthenticationService = Application.Interfaces.IAuthenticationService;

namespace SICPAI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationAuthenticationService _authenticationService;
        private readonly IActivityLogWriter _activityLogWriter;

        public AuthController(IUserService userService, ApplicationAuthenticationService authenticationService, IActivityLogWriter activityLogWriter)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _activityLogWriter = activityLogWriter;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel(returnUrl) { RememberMe = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authenticationService.ValidateCredentialsAsync(model.Email ?? string.Empty, model.Password ?? string.Empty, HttpContext.RequestAborted);
            if (user == null || !user.ActiveFlag)
            {
                await _activityLogWriter.WriteAsync(new ActivityLog { Action = "Login fallido", EntityName = nameof(User), Description = $"Intento de acceso para {model.Email}.", IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() }, HttpContext.RequestAborted);
                ModelState.AddModelError(string.Empty, "El usuario o la contraseña no son válidos.");
                return View(model);
            }

            var claims = new List<SecurityClaim>
            {
                new SecurityClaim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new SecurityClaim(ClaimTypes.Name, $"{user.Name} {user.LastName}".Trim()),
                new SecurityClaim(ClaimTypes.Email, user.Email)
            };
            claims.AddRange(user.UserRoles.Where(ur => ur.ActiveFlag && ur.RoleCatalog.ActiveFlag)
                .Select(ur => new SecurityClaim(ClaimTypes.Role, ur.RoleCatalog.Name)));
            claims.AddRange(user.UserRoles.Where(ur => ur.ActiveFlag)
                .SelectMany(ur => ur.RoleCatalog.RolePermissions.Where(rp => rp.Active && rp.PermissionCatalog.Active))
                .Select(rp => new SecurityClaim("permission", rp.PermissionCatalog.Code)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });
            await _activityLogWriter.WriteAsync(new ActivityLog { UserId = user.Id, Action = "Login exitoso", EntityName = nameof(User), EntityId = user.Id.ToString(), IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() }, HttpContext.RequestAborted);

            return LocalRedirect(model.ReturnUrl ?? "/Home/Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) await _activityLogWriter.WriteAsync(new ActivityLog { UserId = userId, Action = "Logout", EntityName = nameof(User), EntityId = userId.ToString(), IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() }, HttpContext.RequestAborted);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
            if (!await _authenticationService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, HttpContext.RequestAborted)) return BadRequest(new { error = "La contraseña actual no es válida." });
            await _activityLogWriter.WriteAsync(new ActivityLog { UserId = userId, Action = "Cambio de contraseña", EntityName = nameof(User), EntityId = userId.ToString() }, HttpContext.RequestAborted);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Contraseña actualizada. Inicie sesión nuevamente." });
        }

        public IActionResult AccesoDenegado() => View();

        public IActionResult Index() => RedirectToAction(nameof(Login));
    }
}
