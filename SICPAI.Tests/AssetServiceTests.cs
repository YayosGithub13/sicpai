using Application.Interfaces;
using Application.Services;
using Domain.DTOs;
using Domain.Entities;

namespace SICPAI.Tests;

public class AssetServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenCodeExists_ThrowsInvalidOperationException()
    {
        var repository = new FakeAssetRepository { ExistsCode = true };
        var logWriter = new FakeActivityLogWriter();
        var service = new AssetService(repository, logWriter);

        var dto = new CreateAssetDto("EQ-001", "BC-001", "Laptop", "Cómputo", null, null, null, "Bueno", "Activo", "TI", "User", "Oficina", null, 1000);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto, 1));
    }

    [Fact]
    public async Task RetireAsync_WhenReasonIsEmpty_ThrowsArgumentException()
    {
        var repository = new FakeAssetRepository
        {
            AssetById = new Asset { Id = 7, AssetCode = "EQ-007", Name = "Monitor", AssetType = "Periférico" }
        };
        var logWriter = new FakeActivityLogWriter();
        var service = new AssetService(repository, logWriter);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RetireAsync(7, " ", 2));
    }

    [Fact]
    public async Task AttachDocumentAsync_WhenAssetExists_ReturnsAttachmentWithApiRelativePath()
    {
        var repository = new FakeAssetRepository
        {
            AssetById = new Asset { Id = 3, AssetCode = "EQ-003", Name = "Impresora", AssetType = "Periférico" }
        };
        var logWriter = new FakeActivityLogWriter();
        var service = new AssetService(repository, logWriter);

        var result = await service.AttachDocumentAsync(3, "archivo.pdf", "stored.pdf", "/uploads/stored.pdf", "application/pdf", 120, 10);

        Assert.NotNull(result);
        Assert.Equal("/api/assets/3/attachments/1/download", result!.RelativePath);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    private sealed class FakeAssetRepository : IAssetRepository
    {
        public bool ExistsCode { get; set; }
        public Asset? AssetById { get; set; }
        public int SaveChangesCalls { get; private set; }

        public Task<IReadOnlyList<Asset>> ListAsync(string? search, string? status, string? area, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Asset>>(new List<Asset>());

        public Task<Asset?> GetAsync(int id, bool includeHistory = false, CancellationToken cancellationToken = default)
            => Task.FromResult(AssetById);

        public Task<Asset?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult<Asset?>(null);

        public Task<IReadOnlyList<AssetMovement>> ListMovementsAsync(string? search, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AssetMovement>>(new List<AssetMovement>());

        public Task<AssetAttachment?> GetAttachmentAsync(int assetId, int attachmentId, CancellationToken cancellationToken = default)
            => Task.FromResult<AssetAttachment?>(null);

        public Task<bool> ExistsCodeAsync(string code, int? excludedId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistsCode);

        public Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            AssetById = asset;
            return Task.CompletedTask;
        }

        public Task AddAttachmentAsync(AssetAttachment attachment, CancellationToken cancellationToken = default)
        {
            attachment.Id = 1;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeActivityLogWriter : IActivityLogWriter
    {
        public readonly List<ActivityLog> Logs = [];

        public Task WriteAsync(ActivityLog log, CancellationToken cancellationToken = default)
        {
            Logs.Add(log);
            return Task.CompletedTask;
        }
    }
}
