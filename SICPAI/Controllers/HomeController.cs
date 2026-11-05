using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SICPAI.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SICPAI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/dashboard/summary")]
        public async Task<IActionResult> Summary()
        {
            var byStatus = await _db.Assets.AsNoTracking().GroupBy(a => a.Status).Select(g => new { status = g.Key, total = g.Count() }).ToListAsync();
            var byArea = await _db.Assets.AsNoTracking().GroupBy(a => a.Area ?? "Sin área").Select(g => new { area = g.Key, total = g.Count() }).OrderByDescending(x => x.total).Take(8).ToListAsync();
            return Ok(new { total = await _db.Assets.CountAsync(), active = await _db.Assets.CountAsync(a => a.Status == "Activo"), repair = await _db.Assets.CountAsync(a => a.Status == "En reparación"), missing = await _db.Assets.CountAsync(a => a.Status == "Extraviado"), byStatus, byArea, pendingTransfers = await _db.AssetTransfers.CountAsync(t => t.Status == "Pendiente"), unreadNotifications = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) is { } claim && int.TryParse(claim.Value, out var userId) ? await _db.Notifications.CountAsync(n => n.UserId == userId && !n.Read) : 0 });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
