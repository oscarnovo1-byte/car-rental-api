using CarRental.Application.Abstractions;

namespace CarRental.Application.Rentals.GetRentals;

public sealed class GetRentalsQueryHandler
{
    private readonly IRentalRepository _rentalRepository;

    public GetRentalsQueryHandler(
        IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task<IReadOnlyList<RentalListItemResponse>> HandleAsync(
        GetRentalsQuery query,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalRepository.GetAllAsync(
            cancellationToken);

        return rentals
            .Select(rental => new RentalListItemResponse(
                rental.Id,
                rental.CustomerId,
                rental.CarId,
                rental.StartDate,
                rental.EndDate,
                rental.Status))
            .ToList();
    }
}