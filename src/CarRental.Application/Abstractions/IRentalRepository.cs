using CarRental.Domain.Entities;

namespace CarRental.Application.Abstractions;

public interface IRentalRepository
{
    Task<bool> HasOverlappingRentalAsync(
        Guid carId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Rental rental,
        CancellationToken cancellationToken = default);
}