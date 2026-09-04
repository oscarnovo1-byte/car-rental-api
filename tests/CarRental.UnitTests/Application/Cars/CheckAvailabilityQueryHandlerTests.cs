using CarRental.Application.Abstractions;
using CarRental.Application.Cars.CheckAvailability;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using Moq;

namespace CarRental.UnitTests.Application.Cars;

public sealed class CheckAvailabilityQueryHandlerTests
{
    private readonly Mock<ICarAvailabilityQuery> _availabilityQueryMock;
    private readonly CheckAvailabilityQueryHandler _handler;

    public CheckAvailabilityQueryHandlerTests()
    {
        _availabilityQueryMock = new Mock<ICarAvailabilityQuery>();

        _handler = new CheckAvailabilityQueryHandler(
            _availabilityQueryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAvailableCars_WhenPeriodIsValid()
    {
        // Arrange
        var query = new CheckAvailabilityQuery(
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15),
            CarType.Suv);

        var availableCars = new List<AvailableCarDto>
        {
            new(Guid.NewGuid(), CarType.Suv, "Toyota RAV4"),
            new(Guid.NewGuid(), CarType.Suv, "Honda CR-V")
        };

        _availabilityQueryMock
            .Setup(x => x.GetAvailableAsync(
                query.StartDate,
                query.EndDate,
                query.Type,
                query.Model,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableCars);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(availableCars, result);

        _availabilityQueryMock.Verify(
            x => x.GetAvailableAsync(
                query.StartDate,
                query.EndDate,
                query.Type,
                query.Model,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowDomainException_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var query = new CheckAvailabilityQuery(
            new DateOnly(2026, 9, 15),
            new DateOnly(2026, 9, 10));

        // Act
        var action = () => _handler.HandleAsync(query);

        // Assert
        await Assert.ThrowsAsync<DomainException>(action);

        _availabilityQueryMock.Verify(
            x => x.GetAvailableAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CarType?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowDomainException_WhenStartDateEqualsEndDate()
    {
        // Arrange
        var date = new DateOnly(2026, 9, 10);

        var query = new CheckAvailabilityQuery(
            date,
            date);

        // Act
        var action = () => _handler.HandleAsync(query);

        // Assert
        await Assert.ThrowsAsync<DomainException>(action);

        _availabilityQueryMock.Verify(
            x => x.GetAvailableAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CarType?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}