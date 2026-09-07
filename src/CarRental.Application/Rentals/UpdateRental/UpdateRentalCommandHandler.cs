using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Rentals.UpdateRental;

public sealed class UpdateRentalCommandHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly ICarRepository _carRepository;

    public UpdateRentalCommandHandler(
        IRentalRepository rentalRepository,
        ICarRepository carRepository)
    {
        _rentalRepository = rentalRepository;
        _carRepository = carRepository;
    }

    public async Task HandleAsync(
        UpdateRentalCommand command,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepository.GetForUpdateAsync(
            command.RentalId,
            cancellationToken);

        if (rental is null)
        {
            throw new NotFoundException(
                $"Rental '{command.RentalId}' was not found.");
        }

        var car = await _carRepository.GetByIdAsync(
            command.CarId,
            cancellationToken);

        if (car is null)
        {
            throw new NotFoundException(
                $"Car '{command.CarId}' was not found.");
        }

        var hasOverlap =
            await _rentalRepository.HasOverlappingRentalAsync(
                command.CarId,
                command.StartDate,
                command.EndDate,
                command.RentalId,
                cancellationToken);

        if (hasOverlap)
        {
            throw new ConflictException(
                "The car is not available for the selected period.");
        }

        rental.Update(
            command.CarId,
            command.StartDate,
            command.EndDate);

        await _rentalRepository.SaveChangesAsync(
            cancellationToken);
    }
}