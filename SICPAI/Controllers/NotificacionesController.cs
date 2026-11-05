using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/notificaciones")]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotificacionesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool unreadOnly = false)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        var query = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId.Value);
        if (unreadOnly) query = query.Where(n => !n.Read);
        return Ok(await query.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync());
    }

    [HttpPatch("{id:long}/leer")]
    public async Task<IActionResult> MarkRead(long id)
    {
        var userId = CurrentUserId();
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification == null) return NotFound();
        notification.Read = true;
        await _db.SaveChangesAsync();
        return Ok(notification);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
