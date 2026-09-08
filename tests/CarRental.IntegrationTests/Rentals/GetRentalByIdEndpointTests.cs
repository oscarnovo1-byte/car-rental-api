using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Rentals.GetRentalById;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Rentals;

public sealed class GetRentalByIdEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetRentalByIdEndpointTests(
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
    public async Task GetRentalById_WhenRentalExists_ShouldReturnRental()
    {
        var customer = new Customer(
            "John Doe",
            "Main Street 123",
            "john@example.com");

        var car = new Car(
            CarType.Suv,
            "Toyota RAV4");

        var rental = new Rental(
            customer.Id,
            car.Id,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        await _factory.SeedAsync(db =>
        {
            db.Customers.Add(customer);
            db.Cars.Add(car);
            db.Rentals.Add(rental);

            return Task.CompletedTask;
        });

        var response = await _client.GetAsync(
            $"/api/rentals/{rental.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<RentalResponse>();

        Assert.NotNull(result);
        Assert.Equal(rental.Id, result.Id);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(car.Id, result.CarId);
        Assert.Equal(new DateOnly(2026, 9, 10), result.StartDate);
        Assert.Equal(new DateOnly(2026, 9, 15), result.EndDate);
        Assert.Equal(RentalStatus.Reserved, result.Status);
    }

    [Fact]
    public async Task GetRentalById_WhenRentalDoesNotExist_ShouldReturnNotFound()
    {
        var rentalId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/rentals/{rentalId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}