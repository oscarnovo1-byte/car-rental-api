using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Rentals.GetRentals;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Rentals;

public sealed class GetRentalsEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetRentalsEndpointTests(
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
    public async Task GetRentals_WhenRentalsExist_ShouldReturnRentals()
    {
        var customer1 = new Customer(
            "John Doe",
            "Main Street 123",
            "john@example.com");

        var customer2 = new Customer(
            "Jane Smith",
            "Second Street 456",
            "jane@example.com");

        var car1 = new Car(
            CarType.Suv,
            "Toyota RAV4");

        var car2 = new Car(
            CarType.Compact,
            "Toyota Corolla");

        var rental1 = new Rental(
            customer1.Id,
            car1.Id,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var rental2 = new Rental(
            customer2.Id,
            car2.Id,
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        await _factory.SeedAsync(db =>
        {
            db.Customers.AddRange(
                customer1,
                customer2);

            db.Cars.AddRange(
                car1,
                car2);

            db.Rentals.AddRange(
                rental1,
                rental2);

            return Task.CompletedTask;
        });

        var response = await _client.GetAsync(
            "/api/rentals");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var rentals =
            await response.Content
                .ReadFromJsonAsync<List<RentalListItemResponse>>();

        Assert.NotNull(rentals);
        Assert.Equal(2, rentals.Count);

        Assert.Contains(
            rentals,
            r => r.Id == rental1.Id &&
                 r.CustomerId == customer1.Id &&
                 r.CarId == car1.Id);

        Assert.Contains(
            rentals,
            r => r.Id == rental2.Id &&
                 r.CustomerId == customer2.Id &&
                 r.CarId == car2.Id);
    }

    [Fact]
    public async Task GetRentals_WhenNoRentalsExist_ShouldReturnEmptyList()
    {
        var response = await _client.GetAsync(
            "/api/rentals");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var rentals =
            await response.Content
                .ReadFromJsonAsync<List<RentalListItemResponse>>();

        Assert.NotNull(rentals);
        Assert.Empty(rentals);
    }
}