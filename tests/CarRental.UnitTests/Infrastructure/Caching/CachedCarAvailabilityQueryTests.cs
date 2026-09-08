using CarRental.Application.Abstractions;
using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;
using CarRental.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CarRental.UnitTests.Infrastructure.Caching;

public sealed class CachedCarAvailabilityQueryTests
{
    [Fact]
    public async Task GetAvailableAsync_WithSameParameters_ShouldUseCachedResult()
    {
        // Arrange
        var innerQueryMock = new Mock<ICarAvailabilityQuery>();

        var expectedResult = new List<AvailableCarDto>
        {
            new(
                Guid.NewGuid(),
                CarType.Suv,
                "Toyota RAV4")
        };

        innerQueryMock
            .Setup(x => x.GetAvailableAsync(
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 15),
                CarType.Suv,
                "Toyota RAV4",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        using var memoryCache =
            new MemoryCache(
                new MemoryCacheOptions());

        var cacheState =
            new CarAvailabilityCacheState();

        var decorator =
            new CachedCarAvailabilityQuery(
                innerQueryMock.Object,
                memoryCache,
                cacheState);

        // Act
        var firstResult =
            await decorator.GetAvailableAsync(
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 15),
                CarType.Suv,
                "Toyota RAV4");

        var secondResult =
            await decorator.GetAvailableAsync(
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 15),
                CarType.Suv,
                "Toyota RAV4");

        // Assert
        Assert.Same(expectedResult, firstResult);
        Assert.Same(expectedResult, secondResult);

        innerQueryMock.Verify(
            x => x.GetAvailableAsync(
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 15),
                CarType.Suv,
                "Toyota RAV4",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}