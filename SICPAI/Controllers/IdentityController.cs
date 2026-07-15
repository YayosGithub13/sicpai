using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SICPAI.Models;
using SICPAI.Helpers;

namespace SICPAI.Controllers
{
    public class IdentityController : Controller
    {
        private readonly IUserService _userService;

        public IdentityController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [Route("/Identity/Login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new LoginViewModel(returnUrl));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Login), viewModel);
            }

            var email = viewModel.Email;
            var usuario = await _userService.ValidateUserAsync(email!, viewModel.Password!);

            if (usuario is null)
            {
                ModelState.AddModelError(nameof(viewModel.Password), "Usuario y/o contraseña incorrecta.");
                return View(nameof(Login), viewModel);
            }

            await SignInAsync(usuario);

            // If you need force-change-password behavior, add the property to User and handle here.

            if (Url.IsLocalUrl(viewModel.ReturnUrl))
            {
                return Redirect(viewModel.ReturnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Dashboard");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(AuthController.Login), "Auth");
        }

        private async Task SignInAsync(User usuario)
        {
            var claims = ClaimsHelper.GetUserClaims(usuario);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        // NOTE: Change password / reset flows depend on IUserService support.
        // Implement those actions when the service exposes the needed methods.
    }
}
