using Microsoft.AspNetCore.Mvc;

namespace SICPAI.Controllers
{
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Financiero()
        {
            return View();
        }

        public IActionResult Analytics()
        {
            return View();
        }
    }
}
