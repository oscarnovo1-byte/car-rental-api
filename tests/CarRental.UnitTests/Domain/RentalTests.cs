using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Exceptions;

namespace CarRental.UnitTests.Domain;

public sealed class RentalTests
{
    #region Constructor Tests
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
    #endregion

    #region Cancel Method Tests
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
    #endregion

    #region Update Method Tests
    [Fact]
    public void Update_ShouldModifyRental_WhenDataIsValid()
    {
        // Arrange
        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var newCarId = Guid.NewGuid();
        var newStartDate = new DateOnly(2026, 9, 20);
        var newEndDate = new DateOnly(2026, 9, 25);

        // Act
        rental.Update(
            newCarId,
            newStartDate,
            newEndDate);

        // Assert
        Assert.Equal(newCarId, rental.CarId);
        Assert.Equal(newStartDate, rental.StartDate);
        Assert.Equal(newEndDate, rental.EndDate);
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenRentalIsCancelled()
    {
        // Arrange
        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        rental.Cancel();

        // Act
        var act = () => rental.Update(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 25));

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenEndDateIsNotAfterStartDate()
    {
        // Arrange
        var rental = new Rental(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        var newCarId = Guid.NewGuid();
        var newStartDate = new DateOnly(2026, 9, 20);
        var newEndDate = new DateOnly(2026, 9, 20);

        // Act
        var act = () => rental.Update(
            newCarId,
            newStartDate,
            newEndDate);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Update_ShouldNotModifyRental_WhenValidationFails()
    {
        // Arrange
        var originalCarId = Guid.NewGuid();
        var originalStartDate = new DateOnly(2026, 9, 10);
        var originalEndDate = new DateOnly(2026, 9, 15);

        var rental = new Rental(
            Guid.NewGuid(),
            originalCarId,
            originalStartDate,
            originalEndDate);

        // Act
        try
        {
            rental.Update(
                Guid.NewGuid(),
                new DateOnly(2026, 9, 25),
                new DateOnly(2026, 9, 20));
        }
        catch (DomainException)
        {
        }

        // Assert
        Assert.Equal(originalCarId, rental.CarId);
        Assert.Equal(originalStartDate, rental.StartDate);
        Assert.Equal(originalEndDate, rental.EndDate);
    }
    #endregion
}