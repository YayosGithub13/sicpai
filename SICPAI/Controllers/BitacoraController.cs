using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/bitacora")]
public class BitacoraController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    public BitacoraController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? action, [FromQuery] string? entity, DateTime? from, DateTime? to, int page = 1, int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await _auditLogService.ListAsync(action, entity, from, to, page, pageSize, HttpContext.RequestAborted);
        return Ok(new { page, pageSize, total = result.Total, items = result.Items });
    }
}
