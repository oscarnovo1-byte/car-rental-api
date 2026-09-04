using CarRental.Domain.Enums;

namespace CarRental.Application.Cars.CheckAvailability;

public sealed record AvailableCarDto(
    Guid Id,
    CarType Type,
    string Model);