using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;
using CarRental.Application.Rentals.GetRentalById;
using CarRental.Domain.Entities;
using Moq;

namespace CarRental.UnitTests.Application.Rentals;

public sealed class GetRentalByIdQueryHandlerTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly GetRentalByIdQueryHandler _handler;

    public GetRentalByIdQueryHandlerTests()
    {
        _rentalRepositoryMock = new Mock<IRentalRepository>();

        _handler = new GetRentalByIdQueryHandler(
            _rentalRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnRental_WhenRentalExists()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var carId = Guid.NewGuid();

        var rental = new Rental(
            customerId,
            carId,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        var query = new GetRentalByIdQuery(rentalId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(rental.Id, result.Id);
        Assert.Equal(rental.CustomerId, result.CustomerId);
        Assert.Equal(rental.CarId, result.CarId);
        Assert.Equal(rental.StartDate, result.StartDate);
        Assert.Equal(rental.EndDate, result.EndDate);
        Assert.Equal(rental.Status, result.Status);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenRentalDoesNotExist()
    {
        // Arrange
        var rentalId = Guid.NewGuid();

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        var query = new GetRentalByIdQuery(rentalId);

        // Act
        var act = async () => await _handler.HandleAsync(query);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}