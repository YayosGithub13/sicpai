namespace Domain.DTOs;

public sealed record CreateTransferDto(int AssetId, string ToArea, string? ToResponsible, string? ToLocation, string Reason);
public sealed record RejectTransferDto(string? Reason);
