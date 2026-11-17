using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Interfaces;

namespace SICPAI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly ICatalogOperations _catalogOperations;
    public CatalogosController(ICatalogService catalogService, ICatalogOperations catalogOperations)
    {
        _catalogService = catalogService;
        _catalogOperations = catalogOperations;
    }

    [HttpGet("areas")]
    public async Task<IActionResult> Areas([FromQuery] bool includeInactive = false) => Ok(await _catalogService.GetAreasAsync(includeInactive, HttpContext.RequestAborted));

    [HttpPost("areas")]
    public async Task<IActionResult> CreateArea([FromBody] AreaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "El código y nombre del área son obligatorios." });
        try
        {
            var area = await _catalogOperations.CreateAreaAsync(new InstitutionalArea { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description?.Trim(), ParentAreaId = request.ParentAreaId }, HttpContext.RequestAborted);
            return Created($"/api/catalogos/areas/{area!.Id}", area);
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpPut("areas/{id:int}")]
    public async Task<IActionResult> UpdateArea(int id, [FromBody] AreaRequest request)
    {
        try
        {
            var area = await _catalogOperations.UpdateAreaAsync(id, new InstitutionalArea { Code = request.Code, Name = request.Name, Description = request.Description, ParentAreaId = request.ParentAreaId, Active = request.Active }, HttpContext.RequestAborted);
            return area is null ? NotFound() : Ok(area);
        }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpGet("ubicaciones")]
    public async Task<IActionResult> Locations([FromQuery] bool includeInactive = false) => Ok(await _catalogService.GetLocationsAsync(includeInactive, HttpContext.RequestAborted));

    [HttpGet("responsables")]
    public async Task<IActionResult> Responsables([FromQuery] bool includeInactive = false) => Ok(await _catalogOperations.GetResponsablesAsync(includeInactive, HttpContext.RequestAborted));

    [HttpPost("responsables")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResponsable([FromBody] ResponsableRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Nombre completo y correo son obligatorios." });

        try
        {
            var responsable = await _catalogOperations.CreateResponsableAsync(request.FullName, request.UserName, request.Email, request.Active, HttpContext.RequestAborted);
            return Created("/api/catalogos/responsables", responsable);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { error = exception.Message });
        }
    }

    [HttpPut("responsables/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateResponsable(int id, [FromBody] ResponsableRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Nombre completo y correo son obligatorios." });

        try
        {
            var responsable = await _catalogOperations.UpdateResponsableAsync(id, request.FullName, request.UserName, request.Email, request.Active, HttpContext.RequestAborted);
            return responsable is null ? NotFound() : Ok(responsable);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { error = exception.Message });
        }
    }

    [HttpPost("ubicaciones")]
    public async Task<IActionResult> CreateLocation([FromBody] LocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Site) || string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Código, sede y nombre son obligatorios." });
        try
        {
            var location = await _catalogOperations.CreateLocationAsync(new PhysicalLocation { Code = request.Code.Trim(), Site = request.Site.Trim(), Building = request.Building?.Trim(), Floor = request.Floor?.Trim(), Name = request.Name.Trim(), Description = request.Description?.Trim() }, HttpContext.RequestAborted);
            return Created($"/api/catalogos/ubicaciones/{location!.Id}", location);
        }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpPut("ubicaciones/{id:int}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationRequest request)
    {
        try
        {
            var location = await _catalogOperations.UpdateLocationAsync(id, new PhysicalLocation { Code = request.Code, Site = request.Site, Building = request.Building, Floor = request.Floor, Name = request.Name, Description = request.Description, Active = request.Active }, HttpContext.RequestAborted);
            return location is null ? NotFound() : Ok(location);
        }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    public sealed class AreaRequest { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string? Description { get; set; } public int? ParentAreaId { get; set; } public bool Active { get; set; } = true; }
    public sealed class LocationRequest { public string Code { get; set; } = string.Empty; public string Site { get; set; } = string.Empty; public string? Building { get; set; } public string? Floor { get; set; } public string Name { get; set; } = string.Empty; public string? Description { get; set; } public bool Active { get; set; } = true; }
    public sealed class ResponsableRequest { public string FullName { get; set; } = string.Empty; public string UserName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public bool Active { get; set; } = true; }
}
