using CarRental.Application.Rentals.CreateRental;
using CarRental.Application.Rentals.GetRentalById;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/rentals")]
public sealed class RentalsController : ControllerBase
{
    private readonly CreateRentalCommandHandler _createRentalCommandHandler;
    private readonly GetRentalByIdQueryHandler _getRentalByIdQueryHandler;

    public RentalsController(
        CreateRentalCommandHandler createRentalCommandHandler,
        GetRentalByIdQueryHandler getRentalByIdQueryHandler)
    {
        _createRentalCommandHandler = createRentalCommandHandler;
        _getRentalByIdQueryHandler = getRentalByIdQueryHandler;
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

        return CreatedAtAction(
            nameof(GetRentalById),
            new { id = rentalId },
            new { Id = rentalId });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalResponse>> GetRentalById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRentalByIdQuery(id);

        var rental = await _getRentalByIdQueryHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(rental);
    }
}

public sealed record CreateRentalRequest(
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);