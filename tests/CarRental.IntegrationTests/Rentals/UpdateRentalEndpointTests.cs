using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Rentals.GetRentalById;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Rentals;

public sealed class UpdateRentalEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateRentalEndpointTests(
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
    public async Task UpdateRental_WithValidData_ShouldReturnNoContent()
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

        var updateRequest = new
        {
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 20),
            EndDate = new DateOnly(2026, 9, 25)
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/rentals/{rentalId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync(
    $"/api/rentals/{rentalId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var rental =
            await getResponse.Content.ReadFromJsonAsync<RentalResponse>();

        Assert.NotNull(rental);

        Assert.Equal(
            new DateOnly(2026, 9, 20),
            rental.StartDate);

        Assert.Equal(
            new DateOnly(2026, 9, 25),
            rental.EndDate);
    }

    [Fact]
    public async Task UpdateRental_WhenDatesOverlapAnotherRental_ShouldReturnConflict()
    {
        var customer1 = new Customer(
            "John Doe",
            "Main Street 123",
            "john@example.com");

        var customer2 = new Customer(
            "Jane Smith",
            "Second Street 456",
            "jane@example.com");

        var car = new Car(
            CarType.Suv,
            "Toyota RAV4");

        await _factory.SeedAsync(db =>
        {
            db.Customers.AddRange(customer1, customer2);
            db.Cars.Add(car);

            return Task.CompletedTask;
        });

        var firstRentalRequest = new
        {
            CustomerId = customer1.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            firstRentalRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.NotNull(firstResponse.Headers.Location);

        var secondRentalRequest = new
        {
            CustomerId = customer2.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 20),
            EndDate = new DateOnly(2026, 9, 25)
        };

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            secondRentalRequest);

        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.NotNull(secondResponse.Headers.Location);

        var secondRentalId = secondResponse.Headers.Location
            .Segments
            .Last();

        var updateRequest = new
        {
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 12),
            EndDate = new DateOnly(2026, 9, 18)
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/rentals/{secondRentalId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
    }
}