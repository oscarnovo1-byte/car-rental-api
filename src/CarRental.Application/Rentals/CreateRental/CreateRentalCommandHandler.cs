using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;
using CarRental.Domain.Entities;

namespace CarRental.Application.Rentals.CreateRental;

public sealed class CreateRentalCommandHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICarRepository _carRepository;
    private readonly IRentalRepository _rentalRepository;

    public CreateRentalCommandHandler(
        ICustomerRepository customerRepository,
        ICarRepository carRepository,
        IRentalRepository rentalRepository)
    {
        _customerRepository = customerRepository;
        _carRepository = carRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<Guid> HandleAsync(
        CreateRentalCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(
            command.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                $"Customer '{command.CustomerId}' was not found.");
        }

        var car = await _carRepository.GetByIdAsync(
            command.CarId,
            cancellationToken);

        if (car is null)
        {
            throw new NotFoundException(
                $"Car '{command.CarId}' was not found.");
        }

        var hasOverlap = await _rentalRepository.HasOverlappingRentalAsync(
            carId: command.CarId,
            startDate: command.StartDate,
            endDate: command.EndDate,
            cancellationToken: cancellationToken);

        if (hasOverlap)
        {
            throw new ConflictException(
                "The car is not available for the selected period.");
        }

        var rental = new Rental(
            command.CustomerId,
            command.CarId,
            command.StartDate,
            command.EndDate);

        await _rentalRepository.AddAsync(
            rental,
            cancellationToken);

        return rental.Id;
    }
}