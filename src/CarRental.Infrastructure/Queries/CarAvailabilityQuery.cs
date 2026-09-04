using CarRental.Application.Abstractions;
using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Queries;

public sealed class CarAvailabilityQuery : ICarAvailabilityQuery
{
    private readonly CarRentalDbContext _dbContext;

    public CarAvailabilityQuery(CarRentalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AvailableCarDto>> GetAvailableAsync(
        DateOnly startDate,
        DateOnly endDate,
        CarType? type,
        string? model,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Cars
            .AsNoTracking()
            .Where(car =>
                !_dbContext.Rentals.Any(rental =>
                    rental.CarId == car.Id &&
                    rental.Status == RentalStatus.Reserved &&
                    startDate < rental.EndDate &&
                    endDate > rental.StartDate))
            .Where(car =>
                !car.Services.Any(service =>
                    service.Date >= startDate &&
                    service.Date < endDate));

        if (type.HasValue)
        {
            query = query.Where(car => car.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            query = query.Where(car => car.Model == model);
        }

        return await query
            .Select(car => new AvailableCarDto(
                car.Id,
                car.Type,
                car.Model))
            .ToListAsync(cancellationToken);
    }
}