using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;

using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/cars")]
public sealed class CarsController : ControllerBase
{
    private readonly CheckAvailabilityQueryHandler _checkAvailabilityQueryHandler;

    public CarsController(
        CheckAvailabilityQueryHandler checkAvailabilityQueryHandler)
    {
        _checkAvailabilityQueryHandler = checkAvailabilityQueryHandler;
    }

    [HttpGet("availability")]
    public async Task<ActionResult<IReadOnlyList<AvailableCarDto>>> CheckAvailability(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] CarType? type,
        [FromQuery] string? model,
        CancellationToken cancellationToken)
    {
        var query = new CheckAvailabilityQuery(
            startDate,
            endDate,
            type,
            model);

        var cars = await _checkAvailabilityQueryHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(cars);
    }
}