using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/notificaciones")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public NotificacionesController(INotificationService notificationService) => _notificationService = notificationService;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool unreadOnly = false)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _notificationService.ListAsync(userId.Value, unreadOnly, HttpContext.RequestAborted));
    }

    [HttpPatch("{id:long}/leer")]
    public async Task<IActionResult> MarkRead(long id)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        var notification = await _notificationService.MarkReadAsync(id, userId.Value, HttpContext.RequestAborted);
        if (notification == null) return NotFound();
        return Ok(notification);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
