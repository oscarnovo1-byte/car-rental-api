using CarRental.Application.Cars.CheckAvailability;
using CarRental.Application.Customers.CreateCustomer;
using CarRental.Application.Rentals.CancelRental;
using CarRental.Application.Rentals.CreateRental;
using CarRental.Application.Rentals.GetRentalById;
using CarRental.Application.Rentals.GetRentals;
using CarRental.Application.Rentals.UpdateRental;

using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRentalCommandHandler>();
        services.AddScoped<CheckAvailabilityQueryHandler>();
        services.AddScoped<GetRentalByIdQueryHandler>();
        services.AddScoped<CancelRentalCommandHandler>();
        services.AddScoped<GetRentalsQueryHandler>();
        services.AddScoped<UpdateRentalCommandHandler>();
        services.AddScoped<CreateCustomerCommandHandler>();
        
        return services;
    }
}