using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/bitacora")]
public class BitacoraController : ControllerBase
{
    private readonly AppDbContext _db;
    public BitacoraController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? action, [FromQuery] string? entity, DateTime? from, DateTime? to, int page = 1, int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.ActivityLogs.AsNoTracking().Include(l => l.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(l => l.Action == action);
        if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(l => l.EntityName == entity);
        if (from.HasValue) query = query.Where(l => l.OccurredAt >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(l => l.OccurredAt < to.Value.ToUniversalTime().AddDays(1));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { page, pageSize, total, items });
    }
}
