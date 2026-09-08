using CarRental.Application.Abstractions;

namespace CarRental.Infrastructure.Caching;

public sealed class CarAvailabilityCacheState
    : ICarAvailabilityCacheInvalidator
{
    private long _version;

    public long Version =>
        Interlocked.Read(ref _version);

    public void Invalidate()
    {
        Interlocked.Increment(ref _version);
    }
}