using System.Net;
using System.Net.Http.Json;

using CarRental.IntegrationTests.Infrastructure;
using CarRental.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.IntegrationTests.Customers;

public sealed class CreateCustomerEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateCustomerEndpointTests(
        CarRentalWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new
        {
            FullName = "John Doe",
            Address = "Main Street 123",
            Email = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/customers",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result =
            await response.Content.ReadFromJsonAsync<CreateCustomerResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        using var scope = _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

        var customer = await dbContext.Customers
            .SingleAsync(x => x.Id == result.Id);

        Assert.Equal("John Doe", customer.FullName);
        Assert.Equal("Main Street 123", customer.Address);
        Assert.Equal("john@example.com", customer.Email);
    }

    private sealed record CreateCustomerResponse(Guid Id);
}