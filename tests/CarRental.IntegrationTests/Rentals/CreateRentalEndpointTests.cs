using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Rentals.GetRentalById;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Rentals;

public sealed class CreateRentalEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateRentalEndpointTests(
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
    public async Task CreateRental_WithValidData_ShouldReturnCreated()
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

        var request = new
        {
            CustomerId = customer.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/rentals",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var getResponse = await _client.GetAsync(response.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var rental = await getResponse.Content.ReadFromJsonAsync<RentalResponse>();

        Assert.NotNull(rental);
        Assert.Equal(customer.Id, rental.CustomerId);
        Assert.Equal(car.Id, rental.CarId);
        Assert.Equal(new DateOnly(2026, 9, 10), rental.StartDate);
        Assert.Equal(new DateOnly(2026, 9, 15), rental.EndDate);
    }

    [Fact]
    public async Task CreateRental_WhenCarIsAlreadyBooked_ShouldReturnConflict()
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

        var firstRequest = new
        {
            CustomerId = customer1.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            firstRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var overlappingRequest = new
        {
            CustomerId = customer2.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 12),
            EndDate = new DateOnly(2026, 9, 18)
        };

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            overlappingRequest);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateRental_WhenCarIsAvailableAfterExistingRental_ShouldReturnCreated()
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

        var firstRequest = new
        {
            CustomerId = customer1.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            firstRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondRequest = new
        {
            CustomerId = customer2.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 15),
            EndDate = new DateOnly(2026, 9, 20)
        };

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/rentals",
            secondRequest);

        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }
}