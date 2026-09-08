using CarRental.Application.Abstractions;
using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace CarRental.Infrastructure.Caching;

public sealed class CachedCarAvailabilityQuery
    : ICarAvailabilityQuery
{
    private readonly ICarAvailabilityQuery _inner;
    private readonly IMemoryCache _cache;
    private readonly CarAvailabilityCacheState _cacheState;

    public CachedCarAvailabilityQuery(
        ICarAvailabilityQuery inner,
        IMemoryCache cache,
        CarAvailabilityCacheState cacheState)
    {
        _inner = inner;
        _cache = cache;
        _cacheState = cacheState;
    }

    public async Task<IReadOnlyList<AvailableCarDto>> GetAvailableAsync(
        DateOnly startDate,
        DateOnly endDate,
        CarType? type,
        string? model,
        CancellationToken cancellationToken = default)
    {
        var key = BuildCacheKey(
            startDate,
            endDate,
            type,
            model);

        var result = await _cache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(5);

                return await _inner.GetAvailableAsync(
                    startDate,
                    endDate,
                    type,
                    model,
                    cancellationToken);
            });

        return result ?? [];
    }

    private string BuildCacheKey(
        DateOnly startDate,
        DateOnly endDate,
        CarType? type,
        string? model)
    {
        var normalizedModel =
            model?.Trim().ToLowerInvariant() ?? "all";

        return
            $"car-availability:{_cacheState.Version}:" +
            $"{startDate:yyyyMMdd}:" +
            $"{endDate:yyyyMMdd}:" +
            $"{type?.ToString() ?? "all"}:" +
            $"{normalizedModel}";
    }
}