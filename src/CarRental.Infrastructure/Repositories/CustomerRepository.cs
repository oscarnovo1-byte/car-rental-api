
using CarRental.Application.Abstractions;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CarRentalDbContext _dbContext;

    public CustomerRepository(CarRentalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .FirstOrDefaultAsync(
                c => c.Id == customerId,
                cancellationToken);
    }

    public async Task AddAsync(
    Customer customer,
    CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(
            customer,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}