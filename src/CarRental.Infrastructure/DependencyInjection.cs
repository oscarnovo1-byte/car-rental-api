using CarRental.Application.Abstractions;
using CarRental.Infrastructure.Caching;
using CarRental.Infrastructure.Persistence;
using CarRental.Infrastructure.Queries;
using CarRental.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        services.AddMemoryCache();

        services.AddScoped<CarAvailabilityQuery>();

        services.AddSingleton<CarAvailabilityCacheState>();

        services.AddSingleton<ICarAvailabilityCacheInvalidator>(
            sp => sp.GetRequiredService<CarAvailabilityCacheState>());

        services.AddScoped<ICarAvailabilityQuery>(sp =>
        {
            var query =
                sp.GetRequiredService<CarAvailabilityQuery>();

            var cache =
                sp.GetRequiredService<IMemoryCache>();

            var cacheState =
                sp.GetRequiredService<CarAvailabilityCacheState>();

            return new CachedCarAvailabilityQuery(
                query,
                cache,
                cacheState);
        });

        return services;
    }
}