using CarRental.Application.Abstractions;
using CarRental.Infrastructure.Persistence;
using CarRental.Infrastructure.Queries;
using CarRental.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CarRentalDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<ICarAvailabilityQuery, CarAvailabilityQuery>();

        return services;
    }
}