using Microsoft.AspNetCore.Mvc;

namespace SICPAI.Controllers
{
    public class AdminController : Controller
    {
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
    }
}
