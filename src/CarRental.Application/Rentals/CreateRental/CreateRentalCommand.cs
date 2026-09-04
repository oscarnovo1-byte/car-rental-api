namespace CarRental.Application.Rentals.CreateRental;

public sealed record CreateRentalCommand(
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);