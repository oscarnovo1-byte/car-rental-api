using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;

namespace CarRental.Application.Abstractions;

public interface ICarAvailabilityQuery
{
    Task<IReadOnlyList<AvailableCarDto>> GetAvailableAsync(
        DateOnly startDate,
        DateOnly endDate,
        CarType? type,
        string? model,
        CancellationToken cancellationToken = default);
}