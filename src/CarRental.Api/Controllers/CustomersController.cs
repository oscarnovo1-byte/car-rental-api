using CarRental.Application.Customers.CreateCustomer;

using Microsoft.AspNetCore.Mvc;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly CreateCustomerCommandHandler _createCustomerCommandHandler;

    public CustomersController(
        CreateCustomerCommandHandler createCustomerCommandHandler)
    {
        _createCustomerCommandHandler = createCustomerCommandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FullName,
            request.Address,
            request.Email);

        var customerId =
            await _createCustomerCommandHandler.HandleAsync(
                command,
                cancellationToken);

        return Created(
            $"/api/customers/{customerId}",
            new { Id = customerId });
    }
}

public sealed record CreateCustomerRequest(
    string FullName,
    string Address,
    string Email);