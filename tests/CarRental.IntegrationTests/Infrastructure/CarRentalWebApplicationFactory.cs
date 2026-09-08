using CarRental.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.IntegrationTests.Infrastructure;

public sealed class CarRentalWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<CarRentalDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<CarRentalDbContext>(options =>
                options.UseSqlite(_connection));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

            dbContext.Database.EnsureCreated();
        });
    }

    public async Task SeedAsync(Func<CarRentalDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

        await seed(dbContext);

        await dbContext.SaveChangesAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}