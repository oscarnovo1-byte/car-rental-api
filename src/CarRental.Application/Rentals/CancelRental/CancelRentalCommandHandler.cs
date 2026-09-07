using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Rentals.CancelRental;

public sealed class CancelRentalCommandHandler
{
    private readonly IRentalRepository _rentalRepository;

    public CancelRentalCommandHandler(
        IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task HandleAsync(
        CancelRentalCommand command,
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

        rental.Cancel();

        await _rentalRepository.SaveChangesAsync(
            cancellationToken);
    }
}