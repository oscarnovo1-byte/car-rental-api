using CarRental.Application.Abstractions;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
namespace CarRental.Infrastructure.Repositories;

public sealed class CarRepository : ICarRepository
{
    private readonly CarRentalDbContext _dbContext;

    public CarRepository(CarRentalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Car?> GetByIdAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Cars
            .FirstOrDefaultAsync(
                c => c.Id == carId,
                cancellationToken);
    }
}