using Domain.Entities;
using Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SICPAI.Security;
using Application.Interfaces;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/transferencias")]
public class TransferenciasController : ControllerBase
{
    private readonly ITransferService _transferService;
    public TransferenciasController(ITransferService transferService) => _transferService = transferService;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        return Ok(await _transferService.ListAsync(status, HttpContext.RequestAborted));
    }

    [HttpPost]
    [PermissionAuthorize("assets.update")]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.ToArea) || string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { error = "Área destino y motivo son obligatorios." });
        var transfer = await _transferService.CreateAsync(
            new CreateTransferDto(request.AssetId, request.ToArea, request.ToResponsible, request.ToLocation, request.Reason),
            userId.Value,
            HttpContext.RequestAborted);
        if (transfer == null) return NotFound(new { error = "Bien no encontrado." });
        return Created($"/api/transferencias/{transfer.Id}", transfer);
    }

    [HttpPatch("{id:int}/aprobar")]
    [PermissionAuthorize("transfers.approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var approverId = CurrentUserId();
        if (!approverId.HasValue) return Unauthorized();
        AssetTransfer? transfer;
        try
        {
            transfer = await _transferService.ApproveAsync(id, approverId.Value, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        if (transfer == null) return NotFound();
        return Ok(transfer);
    }

    [HttpPatch("{id:int}/rechazar")]
    [PermissionAuthorize("transfers.approve")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectionRequest request)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        AssetTransfer? transfer;
        try
        {
            transfer = await _transferService.RejectAsync(id, userId.Value, request.Reason, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        if (transfer == null) return NotFound();
        return Ok(transfer);
    }

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public sealed class TransferRequest { public int AssetId { get; set; } public string ToArea { get; set; } = string.Empty; public string? ToResponsible { get; set; } public string? ToLocation { get; set; } public string Reason { get; set; } = string.Empty; }
    public sealed class RejectionRequest { public string? Reason { get; set; } }
}
