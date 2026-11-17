using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SICPAI.Models;

namespace SICPAI.Controllers
{
    public class IdentityController : Controller
    {
        public IdentityController() { }

        [AllowAnonymous]
        [Route("/Identity/Login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            return RedirectToAction(nameof(AuthController.Login), "Auth", new { returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/Identity/SignIn")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel viewModel)
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(AuthController.Login), "Auth", new { returnUrl = viewModel.ReturnUrl });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(AuthController.Logout), "Auth");
        }
    }
}
