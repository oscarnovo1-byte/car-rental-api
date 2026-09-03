namespace CarRental.Domain.Entities;

public sealed class Service
{
    public Guid Id { get; private set; }

    public DateOnly Date { get; private set; }

    private Service()
    {
    }

    public Service(DateOnly date)
    {
        Id = Guid.NewGuid();
        Date = date;
    }
}