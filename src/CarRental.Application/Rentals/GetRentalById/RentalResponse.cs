using CarRental.Domain.Enums;

namespace CarRental.Application.Rentals.GetRentalById;

public sealed record RentalResponse(
    Guid Id,
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate,
    RentalStatus Status);