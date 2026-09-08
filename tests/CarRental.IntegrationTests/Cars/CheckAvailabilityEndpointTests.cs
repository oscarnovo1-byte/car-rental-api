using System.Net;
using System.Net.Http.Json;

using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Cars;

public sealed class CheckAvailabilityEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>,
      IAsyncLifetime
{
    private readonly CarRentalWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CheckAvailabilityEndpointTests(
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
    public async Task CheckAvailability_ShouldReturnOk()
    {
        var response = await _client.GetAsync(
            "/api/cars/availability?startDate=2026-09-10&endDate=2026-09-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CheckAvailability_WhenEndDateIsBeforeStartDate_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/cars/availability?startDate=2026-09-15&endDate=2026-09-10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckAvailability_AfterCreatingRental_ShouldInvalidateCache()
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

        var availabilityUrl =
            "/api/cars/availability" +
            "?startDate=2026-09-10" +
            "&endDate=2026-09-15";

        // Primera consulta: carga el resultado en cache
        var firstResponse =
            await _client.GetAsync(availabilityUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var firstResult =
            await firstResponse.Content
                .ReadFromJsonAsync<List<AvailableCarDto>>();

        Assert.NotNull(firstResult);

        Assert.Contains(
            firstResult,
            x => x.Id == car.Id);

        // Creamos un rental para ese mismo auto y período
        var createRequest = new
        {
            CustomerId = customer.Id,
            CarId = car.Id,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 15)
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/rentals",
                createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        // Segunda consulta con exactamente la misma key
        var secondResponse =
            await _client.GetAsync(availabilityUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var secondResult =
            await secondResponse.Content
                .ReadFromJsonAsync<List<AvailableCarDto>>();

        Assert.NotNull(secondResult);

        Assert.DoesNotContain(
            secondResult,
            x => x.Id == car.Id);
    }

    [Fact]
    public async Task CheckAvailability_AfterCancellingRental_ShouldInvalidateCache()
    {
        // Arrange
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

        var availabilityUrl =
            "/api/cars/availability" +
            "?startDate=2026-09-10" +
            "&endDate=2026-09-15";

        // Primera consulta:
        // el rental está activo, por lo tanto el auto NO está disponible.
        // Este resultado queda cacheado.
        var firstResponse =
            await _client.GetAsync(availabilityUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var firstResult =
            await firstResponse.Content
                .ReadFromJsonAsync<List<AvailableCarDto>>();

        Assert.NotNull(firstResult);

        Assert.DoesNotContain(
            firstResult,
            x => x.Id == car.Id);

        // Act
        var cancelResponse =
            await _client.PatchAsync(
                $"/api/rentals/{rental.Id}/cancel",
                null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            cancelResponse.StatusCode);

        // Second query with the exact same parameters.
        // If the cache was properly invalidated,
        // it should query again and find the available car.
        var secondResponse =
            await _client.GetAsync(availabilityUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var secondResult =
            await secondResponse.Content
                .ReadFromJsonAsync<List<AvailableCarDto>>();

        Assert.NotNull(secondResult);

        Assert.Contains(
            secondResult,
            x => x.Id == car.Id);
    }
}