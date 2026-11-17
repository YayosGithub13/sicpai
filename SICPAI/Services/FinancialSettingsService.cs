using System.Collections.Concurrent;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SICPAI.Services;

public interface IFinancialSettingsService
{
    IReadOnlyDictionary<string, decimal> GetDepreciationRates();
    void SetDepreciationRates(IReadOnlyDictionary<string, decimal> rates);
}

public sealed class FinancialSettingsService : IFinancialSettingsService
{
    private const string DepreciationPrefix = "DepreciationRate:";
    private readonly ConcurrentDictionary<string, decimal> _depreciationRates = new(StringComparer.OrdinalIgnoreCase);
    private readonly AppDbContext _db;

    public FinancialSettingsService(IConfiguration configuration, AppDbContext db)
    {
        _db = db;

        var defaults = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["computadora"] = 0.30m,
            ["equipo de cómputo"] = 0.30m,
            ["equipo de computo"] = 0.30m,
            ["vehículo"] = 0.25m,
            ["vehiculo"] = 0.25m,
            ["mobiliario"] = 0.10m,
            ["herramienta"] = 0.20m,
            ["maquinaria"] = 0.10m
        };

        foreach (var pair in defaults)
        {
            _depreciationRates[pair.Key] = pair.Value;
        }

        var section = configuration.GetSection("Financial:DepreciationRates");
        foreach (var child in section.GetChildren())
        {
            if (string.IsNullOrWhiteSpace(child.Key)) continue;
            if (!decimal.TryParse(child.Value, out var rate)) continue;
            _depreciationRates[child.Key.Trim()] = NormalizeRate(rate);
        }

        LoadFromDatabase();
    }

    public IReadOnlyDictionary<string, decimal> GetDepreciationRates()
    {
        LoadFromDatabase();
        return _depreciationRates
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    public void SetDepreciationRates(IReadOnlyDictionary<string, decimal> rates)
    {
        _depreciationRates.Clear();
        foreach (var pair in rates)
        {
            if (string.IsNullOrWhiteSpace(pair.Key)) continue;
            _depreciationRates[pair.Key.Trim()] = NormalizeRate(pair.Value);
        }

        var keys = _depreciationRates.Keys.Select(key => DepreciationPrefix + key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingRows = _db.FinancialSettings.Where(setting => setting.Key.StartsWith(DepreciationPrefix)).ToList();

        foreach (var row in existingRows)
        {
            var catalogKey = row.Key[DepreciationPrefix.Length..];
            if (_depreciationRates.TryGetValue(catalogKey, out var value))
            {
                row.ValueDecimal = value;
                row.UpdatedAt = DateTime.UtcNow;
                keys.Remove(row.Key);
            }
            else
            {
                _db.FinancialSettings.Remove(row);
            }
        }

        foreach (var missingKey in keys)
        {
            var catalogKey = missingKey[DepreciationPrefix.Length..];
            _db.FinancialSettings.Add(new FinancialSetting
            {
                Key = missingKey,
                ValueDecimal = _depreciationRates[catalogKey],
                UpdatedAt = DateTime.UtcNow
            });
        }

        _db.SaveChanges();
    }

    private void LoadFromDatabase()
    {
        var persistedRates = _db.FinancialSettings
            .Where(setting => setting.Key.StartsWith(DepreciationPrefix))
            .AsNoTracking()
            .ToList();

        if (persistedRates.Count == 0) return;

        _depreciationRates.Clear();
        foreach (var row in persistedRates)
        {
            var key = row.Key[DepreciationPrefix.Length..];
            if (string.IsNullOrWhiteSpace(key)) continue;
            _depreciationRates[key] = NormalizeRate(row.ValueDecimal);
        }
    }

    private static decimal NormalizeRate(decimal value) => Math.Min(1m, Math.Max(0m, value));
}
