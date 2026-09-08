using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Rentals.GetRentalById;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Rentals;

public sealed class CancelRentalEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CancelRentalEndpointTests(
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
    public async Task CancelRental_WithExistingRental_ShouldReturnNoContent()
    {
        var customer = new Customer(
            "John Doe",
            "Main Street 123",
            "john@example.com");

        var car = new Car(
            CarType.Suv,
            "Toyota RAV4");

        await _factory.SeedAsync(db =>
        {
            db.Customers.Add(customer);
            db.Cars.Add(car);

            return Task.CompletedTask;
        });

        var createRequest = new
        {
            CustomerId = customer.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var rentalId = createResponse.Headers.Location
            .Segments
            .Last();

        var cancelResponse = await _client.PatchAsync(
            $"/api/rentals/{rentalId}/cancel",
            content: null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            cancelResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/rentals/{rentalId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var rental =
            await getResponse.Content.ReadFromJsonAsync<RentalResponse>();

        Assert.NotNull(rental);
        Assert.Equal(
            RentalStatus.Cancelled,
            rental.Status);
    }

    [Fact]
    public async Task CancelRental_WhenRentalDoesNotExist_ShouldReturnNotFound()
    {
        var rentalId = Guid.NewGuid();

        var response = await _client.PatchAsync(
            $"/api/rentals/{rentalId}/cancel",
            content: null);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}