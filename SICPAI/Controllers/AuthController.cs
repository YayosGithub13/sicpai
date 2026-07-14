using Microsoft.AspNetCore.Mvc;

namespace SICPAI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
