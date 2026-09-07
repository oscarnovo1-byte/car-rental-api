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

    public async Task<Rental?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<Rental?> GetForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentals
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<bool> HasOverlappingRentalAsync(
        Guid carId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludedRentalId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentals.AnyAsync(
            rental =>
                rental.CarId == carId &&
                rental.Status != RentalStatus.Cancelled &&
                (!excludedRentalId.HasValue ||
                 rental.Id != excludedRentalId.Value) &&
                rental.StartDate < endDate &&
                rental.EndDate > startDate,
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

    public async Task<IReadOnlyList<Rental>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentals
            .AsNoTracking()
            .OrderBy(rental => rental.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}