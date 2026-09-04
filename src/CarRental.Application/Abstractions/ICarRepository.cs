using CarRental.Domain.Entities;

namespace CarRental.Application.Abstractions;

public interface ICarRepository
{
    Task<Car?> GetByIdAsync(
        Guid carId,
        CancellationToken cancellationToken = default);
}