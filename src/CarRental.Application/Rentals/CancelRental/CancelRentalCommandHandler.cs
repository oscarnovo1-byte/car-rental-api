using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Rentals.CancelRental;

public sealed class CancelRentalCommandHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly ICarAvailabilityCacheInvalidator _cacheInvalidator;

    public CancelRentalCommandHandler(
        IRentalRepository rentalRepository,
        ICarAvailabilityCacheInvalidator cacheInvalidator)
    {
        _rentalRepository = rentalRepository;
        _cacheInvalidator = cacheInvalidator;
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

        _cacheInvalidator.Invalidate();

        await _rentalRepository.SaveChangesAsync(
            cancellationToken);
    }
}