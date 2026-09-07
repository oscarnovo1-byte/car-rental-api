namespace CarRental.Application.Rentals.UpdateRental;

public sealed record UpdateRentalCommand(
    Guid RentalId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);