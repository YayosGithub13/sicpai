using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SICPAI.Security;
using Application.Interfaces;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/inventarios")]
public class InventariosController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryOperations _inventoryOperations;
    public InventariosController(IInventoryService inventoryService, IInventoryOperations inventoryOperations)
    {
        _inventoryService = inventoryService;
        _inventoryOperations = inventoryOperations;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _inventoryService.ListAsync(HttpContext.RequestAborted));

    [HttpPost]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Start([FromBody] StartRequest request)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue || string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Nombre y usuario son obligatorios." });
        var session = await _inventoryOperations.StartAsync(request.Name, request.Area, userId.Value, HttpContext.RequestAborted);
        if (session is null) return BadRequest(new { error = "No se pudo iniciar el inventario." });
        return Created($"/api/inventarios/{session.Id}", session);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var session = await _inventoryOperations.GetAsync(id, HttpContext.RequestAborted);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpPatch("{id:int}/conteo/{assetId:int}")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Count(int id, int assetId, [FromBody] CountRequest request)
    {
        var valid = new[] { "Encontrado", "Faltante", "Dañado", "Pendiente" };
        if (!valid.Contains(request.Result)) return BadRequest(new { error = "Resultado de conteo no válido." });
        var count = await _inventoryOperations.CountAsync(id, assetId, request.Result, request.Notes, CurrentUserId(), HttpContext.RequestAborted);
        return count == null ? NotFound() : Ok(count);
    }

    [HttpPatch("{id:int}/cerrar")]
    [PermissionAuthorize("audits.manage")]
    public async Task<IActionResult> Close(int id)
    {
        var session = await _inventoryOperations.CloseAsync(id, CurrentUserId(), HttpContext.RequestAborted);
        return session == null ? NotFound() : Ok(session);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public sealed class StartRequest { public string Name { get; set; } = string.Empty; public string? Area { get; set; } }
    public sealed class CountRequest { public string Result { get; set; } = "Encontrado"; public string? Notes { get; set; } }
}
