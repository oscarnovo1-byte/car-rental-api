using CarRental.Application.Abstractions;
using CarRental.Application.Exceptions;
using CarRental.Application.Rentals.CreateRental;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Moq;

namespace CarRental.UnitTests.Application.Rentals;

public sealed class CreateRentalCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICarRepository> _carRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;

    private readonly CreateRentalCommandHandler _handler;

    public CreateRentalCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _carRepositoryMock = new Mock<ICarRepository>();
        _rentalRepositoryMock = new Mock<IRentalRepository>();

        _handler = new CreateRentalCommandHandler(
            _customerRepositoryMock.Object,
            _carRepositoryMock.Object,
            _rentalRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = CreateCommand();

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var action = () => _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);

        _carRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _rentalRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Rental>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenCarDoesNotExist()
    {
        // Arrange
        var command = CreateCommand();

        var customer = new Customer(
            "John Doe",
            "123 Main Street",
            "john@example.com");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        var action = () => _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);

        _rentalRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Rental>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowConflictException_WhenCarIsNotAvailable()
    {
        // Arrange
        var command = CreateCommand();

        var customer = new Customer(
            "John Doe",
            "123 Main Street",
            "john@example.com");

        var car = new Car(
            CarType.Sedan,
            "Toyota Corolla");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _rentalRepositoryMock
            .Setup(x => x.HasOverlappingRentalAsync(
                carId:command.CarId,
                startDate:command.StartDate,
                endDate: command.EndDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.HandleAsync(command);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(action);

        _rentalRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Rental>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateRental_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCommand();

        var customer = new Customer(
            "John Doe",
            "123 Main Street",
            "john@example.com");

        var car = new Car(
            CarType.Sedan,
            "Toyota Corolla");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _carRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.CarId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        _rentalRepositoryMock
            .Setup(x => x.HasOverlappingRentalAsync(
                command.CarId,
                command.StartDate,
                command.EndDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var rentalId = await _handler.HandleAsync(command);

        // Assert
        Assert.NotEqual(Guid.Empty, rentalId);

        _rentalRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Rental>(r =>
                    r.CustomerId == command.CustomerId &&
                    r.CarId == command.CarId &&
                    r.StartDate == command.StartDate &&
                    r.EndDate == command.EndDate &&
                    r.Status == RentalStatus.Reserved),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateRentalCommand CreateCommand()
    {
        return new CreateRentalCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));
    }
}