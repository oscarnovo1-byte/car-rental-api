using CarRental.Domain.Entities;

namespace CarRental.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);
}