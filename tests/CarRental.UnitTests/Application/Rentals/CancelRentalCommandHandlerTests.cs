using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;
using CarRental.Application.Rentals.CancelRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using Moq;

namespace CarRental.UnitTests.Application.Rentals;

public sealed class CancelRentalCommandHandlerTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly CancelRentalCommandHandler _handler;

    public CancelRentalCommandHandlerTests()
    {
        _rentalRepositoryMock = new Mock<IRentalRepository>();

        _handler = new CancelRentalCommandHandler(
            _rentalRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCancelRental_WhenRentalExists()
    {
        // Arrange
        var rentalId = Guid.NewGuid();

        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        var command = new CancelRentalCommand(rentalId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(RentalStatus.Cancelled, rental.Status);

        _rentalRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenRentalDoesNotExist()
    {
        // Arrange
        var rentalId = Guid.NewGuid();

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        var command = new CancelRentalCommand(rentalId);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        _rentalRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowDomainException_WhenRentalIsAlreadyCancelled()
    {
        // Arrange
        var rentalId = Guid.NewGuid();

        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        rental.Cancel();

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        var command = new CancelRentalCommand(rentalId);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        _rentalRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}