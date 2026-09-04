using CarRental.Domain.Enums;

namespace CarRental.Application.Cars.CheckAvailability;

public sealed record CheckAvailabilityQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    CarType? Type = null,
    string? Model = null);