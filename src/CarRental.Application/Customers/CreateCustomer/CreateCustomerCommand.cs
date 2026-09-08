namespace CarRental.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FullName,
    string Address,
    string Email);