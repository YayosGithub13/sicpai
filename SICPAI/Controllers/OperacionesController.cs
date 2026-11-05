using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SICPAI.Controllers
{
    [Authorize]
    public class OperacionesController : Controller
    {
        public IActionResult Transferencias()
        {
            return View();
        }

        public IActionResult Inventarios() => View();

        public IActionResult Responsables()
        {
            return View();
        }

        public IActionResult Areas()
        {
            return View();
        }

        public IActionResult Auditorias()
        {
            return View();
        }

        public IActionResult Escaneo()
        {
            return View();
        }

        public IActionResult Historial()
        {
            return View();
        }

        public IActionResult Ubicaciones()
        {
            return View();
        }
    }
}
