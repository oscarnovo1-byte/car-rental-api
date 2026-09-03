using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public sealed class Rental
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid CarId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public RentalStatus Status { get; private set; }

    private Rental()
    {
    }

    public Rental(
        Guid customerId,
        Guid carId,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException(
                "The rental end date must be after the start date.");
        }

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CarId = carId;
        StartDate = startDate;
        EndDate = endDate;
        Status = RentalStatus.Reserved;
    }

    public void Cancel()
    {
        Status = RentalStatus.Cancelled;
    }
}