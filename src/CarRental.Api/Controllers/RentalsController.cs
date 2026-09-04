using CarRental.Application.Rentals.CreateRental;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/rentals")]
public sealed class RentalsController : ControllerBase
{
    private readonly CreateRentalCommandHandler _createRentalCommandHandler;

    public RentalsController(
        CreateRentalCommandHandler createRentalCommandHandler)
    {
        _createRentalCommandHandler = createRentalCommandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRental(
        CreateRentalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRentalCommand(
            request.CustomerId,
            request.CarId,
            request.StartDate,
            request.EndDate);

        var rentalId = await _createRentalCommandHandler.HandleAsync(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new { Id = rentalId });
    }
}

public sealed record CreateRentalRequest(
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);