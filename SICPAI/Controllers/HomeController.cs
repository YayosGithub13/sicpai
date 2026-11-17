using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SICPAI.Models;
using Application.Interfaces;

namespace SICPAI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/api/dashboard/summary")]
        public async Task<IActionResult> Summary()
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : (int?)null;
            var summary = await _dashboardService.GetSummaryAsync(userId, HttpContext.RequestAborted);
            return Ok(new
            {
                total = summary.Total,
                active = summary.Active,
                repair = summary.Repair,
                missing = summary.Missing,
                byStatus = summary.ByStatus.Select(x => new { status = x.Name, total = x.Total }),
                byArea = summary.ByArea.Select(x => new { area = x.Name, total = x.Total }),
                pendingTransfers = summary.PendingTransfers,
                unreadNotifications = summary.UnreadNotifications
            });
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
