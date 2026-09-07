using CarRental.Domain.Enums;

namespace CarRental.Application.Rentals.GetRentals;

public sealed record RentalListItemResponse(
    Guid Id,
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate,
    RentalStatus Status);