using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;
using CarRental.Application.Rentals.UpdateRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;
using Moq;

namespace CarRental.UnitTests.Application.Rentals;

public sealed class UpdateRentalCommandHandlerTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly UpdateRentalCommandHandler _handler;

    public UpdateRentalCommandHandlerTests()
    {
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _carRepositoryMock = new Mock<ICarRepository>();

        _handler = new UpdateRentalCommandHandler(
            _rentalRepositoryMock.Object,
            _carRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateRental_WhenDataIsValid()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalCarId = Guid.NewGuid();
        var newCarId = Guid.NewGuid();

        var rental = new Rental(
            customerId,
            originalCarId,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var car = new Car(
            CarType.Sedan,
            "2025");

        var command = new UpdateRentalCommand(
            rentalId,
            newCarId,
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                newCarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _rentalRepositoryMock
            .Setup(x => x.HasOverlappingRentalAsync(
                newCarId,
                command.StartDate,
                command.EndDate,
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(newCarId, rental.CarId);
        Assert.Equal(command.StartDate, rental.StartDate);
        Assert.Equal(command.EndDate, rental.EndDate);

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

        var command = new UpdateRentalCommand(
            rentalId,
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

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
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenCarDoesNotExist()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var newCarId = Guid.NewGuid();

        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var command = new UpdateRentalCommand(
            rentalId,
            newCarId,
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                newCarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

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
    public async Task HandleAsync_ShouldThrowConflictException_WhenCarIsNotAvailable()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var newCarId = Guid.NewGuid();

        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var car = new Car(
            CarType.Sedan,
            "2025");

        var command = new UpdateRentalCommand(
            rentalId,
            newCarId,
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                newCarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _rentalRepositoryMock
            .Setup(x => x.HasOverlappingRentalAsync(
                newCarId,
                command.StartDate,
                command.EndDate,
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(act);

        _rentalRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowDomainException_WhenRentalIsCancelled()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var newCarId = Guid.NewGuid();

        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        rental.Cancel();

        var car = new Car(
            CarType.Sedan,
            "2025");

        var command = new UpdateRentalCommand(
            rentalId,
            newCarId,
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        _rentalRepositoryMock
            .Setup(x => x.GetForUpdateAsync(
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                newCarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _rentalRepositoryMock
            .Setup(x => x.HasOverlappingRentalAsync(
                newCarId,
                command.StartDate,
                command.EndDate,
                rentalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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