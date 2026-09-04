using CarRental.Application.Abstractions;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories;

public sealed class RentalRepository : IRentalRepository
{
    private readonly CarRentalDbContext _dbContext;

    public RentalRepository(CarRentalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasOverlappingRentalAsync(
        Guid carId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentals
            .AnyAsync(
                r =>
                    r.CarId == carId &&
                    r.Status == RentalStatus.Reserved &&
                    startDate < r.EndDate &&
                    endDate > r.StartDate,
                cancellationToken);
    }

    public async Task AddAsync(
        Rental rental,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Rentals.AddAsync(
            rental,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}