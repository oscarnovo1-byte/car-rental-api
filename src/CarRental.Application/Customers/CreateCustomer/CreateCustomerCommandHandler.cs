using CarRental.Application.Abstractions;
using CarRental.Domain.Entities;

namespace CarRental.Application.Customers.CreateCustomer;

public sealed class CreateCustomerCommandHandler
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Guid> HandleAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = new Customer(
            command.FullName,
            command.Address,
            command.Email);

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        return customer.Id;
    }
}