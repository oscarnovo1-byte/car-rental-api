using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;

namespace CarRental.UnitTests.Domain;

public sealed class RentalTests
{
    [Fact]
    public void Constructor_ShouldCreateReservedRental_WhenDatesAreValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 9, 10);
        var endDate = new DateOnly(2026, 9, 15);

        // Act
        var rental = new Rental(
            customerId,
            carId,
            startDate,
            endDate);

        // Assert
        Assert.Equal(customerId, rental.CustomerId);
        Assert.Equal(carId, rental.CarId);
        Assert.Equal(startDate, rental.StartDate);
        Assert.Equal(endDate, rental.EndDate);
        Assert.Equal(RentalStatus.Reserved, rental.Status);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 9, 15);
        var endDate = new DateOnly(2026, 9, 10);

        // Act
        var action = () => new Rental(
            customerId,
            carId,
            startDate,
            endDate);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndDateEqualsStartDate()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 10);

        // Act
        var action = () => new Rental(
            customerId,
            carId,
            date,
            date);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        // Act
        rental.Cancel();

        // Assert
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowDomainException_WhenRentalIsAlreadyCancelled()
    {
        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        rental.Cancel();

        var act = () => rental.Cancel();

        Assert.Throws<DomainException>(act);
    }
}