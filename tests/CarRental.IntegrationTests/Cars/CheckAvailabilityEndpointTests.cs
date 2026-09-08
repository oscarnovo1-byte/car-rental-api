using System.Net;

using CarRental.IntegrationTests.Infrastructure;

namespace CarRental.IntegrationTests.Cars;

public sealed class CheckAvailabilityEndpointTests
    : IClassFixture<CarRentalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CheckAvailabilityEndpointTests(
        CarRentalWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
}