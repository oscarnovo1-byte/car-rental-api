using CarRental.Application.Rentals.CancelRental;
using CarRental.Application.Rentals.CreateRental;
using CarRental.Application.Rentals.GetRentalById;
using CarRental.Application.Rentals.UpdateRental;

using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/rentals")]
public sealed class RentalsController : ControllerBase
{
    private readonly CreateRentalCommandHandler _createRentalCommandHandler;
    private readonly GetRentalByIdQueryHandler _getRentalByIdQueryHandler;
    private readonly CancelRentalCommandHandler _cancelRentalCommandHandler;
    private readonly UpdateRentalCommandHandler _updateRentalCommandHandler;

    public RentalsController(
        CreateRentalCommandHandler createRentalCommandHandler,
        GetRentalByIdQueryHandler getRentalByIdQueryHandler,
        CancelRentalCommandHandler cancelRentalCommandHandler,
        UpdateRentalCommandHandler updateRentalCommandHandler)
    {
        _createRentalCommandHandler = createRentalCommandHandler;
        _getRentalByIdQueryHandler = getRentalByIdQueryHandler;
        _cancelRentalCommandHandler = cancelRentalCommandHandler;
        _updateRentalCommandHandler = updateRentalCommandHandler;
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

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelRental(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelRentalCommand(id);

        await _cancelRentalCommandHandler.HandleAsync(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRental(
    Guid id,
    UpdateRentalRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateRentalCommand(
            id,
            request.CarId,
            request.StartDate,
            request.EndDate);

        await _updateRentalCommandHandler.HandleAsync(
            command,
            cancellationToken);

        return NoContent();
    }
}

public sealed record UpdateRentalRequest(
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);
    
public sealed record CreateRentalRequest(
    Guid CustomerId,
    Guid CarId,
    DateOnly StartDate,
    DateOnly EndDate);