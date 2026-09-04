using CarRental.Application.Abstractions;
using CarRental.Domain.Exceptions;

namespace CarRental.Application.Cars.CheckAvailability;

public sealed class CheckAvailabilityQueryHandler
{
    private readonly ICarAvailabilityQuery _availabilityQuery;

    public CheckAvailabilityQueryHandler(
        ICarAvailabilityQuery availabilityQuery)
    {
        _availabilityQuery = availabilityQuery;
    }

    public async Task<IReadOnlyList<AvailableCarDto>> HandleAsync(
        CheckAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.EndDate <= query.StartDate)
        {
            throw new DomainException(
                "The end date must be after the start date.");
        }

        return await _availabilityQuery.GetAvailableAsync(
            query.StartDate,
            query.EndDate,
            query.Type,
            query.Model,
            cancellationToken);
    }
}