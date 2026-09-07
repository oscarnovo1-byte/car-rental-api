using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Rentals.GetRentalById;

public sealed class GetRentalByIdQueryHandler
{
    private readonly IRentalRepository _rentalRepository;

    public GetRentalByIdQueryHandler(
        IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task<RentalResponse> HandleAsync(
        GetRentalByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepository.GetByIdAsync(
            query.RentalId,
            cancellationToken);

        if (rental is null)
        {
            throw new NotFoundException(
                $"Rental '{query.RentalId}' was not found.");
        }

        return new RentalResponse(
            rental.Id,
            rental.CustomerId,
            rental.CarId,
            rental.StartDate,
            rental.EndDate,
            rental.Status);
    }
}