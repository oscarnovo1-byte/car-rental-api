namespace CarRental.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; private set; }

    public string FullName { get; private set; } = null!;

    public string Address { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    private Customer()
    {
    }

    public Customer(
        string fullName,
        string address,
        string email)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Address = address;
        Email = email;
    }
}