using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public sealed class Car
{
    private readonly List<Service> _services = [];

    public Guid Id { get; private set; }

    public CarType Type { get; private set; }

    public string Model { get; private set; } = null!;

    public IReadOnlyCollection<Service> Services => _services;

    private Car()
    {
    }

    public Car(
        CarType type,
        string model)
    {
        Id = Guid.NewGuid();
        Type = type;
        Model = model;
    }
}