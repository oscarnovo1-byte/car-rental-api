using CarRental.Application.Abstractions;
using CarRental.Application.Customers.CreateCustomer;
using CarRental.Domain.Entities;

using Moq;

namespace CarRental.UnitTests.Application.Customers;

public sealed class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _handler = new CreateCustomerCommandHandler(
            _customerRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John Doe",
            "Main Street 123",
            "john@example.com");

        Customer? createdCustomer = null;

        _customerRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>(
                (customer, _) => createdCustomer = customer)
            .Returns(Task.CompletedTask);

        // Act
        var customerId = await _handler.HandleAsync(command);

        // Assert
        Assert.NotEqual(Guid.Empty, customerId);

        Assert.NotNull(createdCustomer);
        Assert.Equal(customerId, createdCustomer.Id);
        Assert.Equal("John Doe", createdCustomer.FullName);
        Assert.Equal("Main Street 123", createdCustomer.Address);
        Assert.Equal("john@example.com", createdCustomer.Email);

        _customerRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}