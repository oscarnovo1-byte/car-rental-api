using CarRental.Infrastructure;
using CarRental.Application;
using CarRental.Api.ExceptionHandling;
using CarRental.Infrastructure.Persistence;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

    dbContext.Database.EnsureCreated();

    if (!dbContext.Cars.Any())
    {
        dbContext.Cars.AddRange(
            new Car(CarType.Suv, "Toyota RAV4"),
            new Car(CarType.Sedan, "Toyota Corolla"),
            new Car(CarType.Compact, "Volkswagen Golf"));

        dbContext.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

    dbContext.Database.EnsureCreated();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;