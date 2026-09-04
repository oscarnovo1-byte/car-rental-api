using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRental.Infrastructure.Persistence.Configurations;

public sealed class CarConfiguration
    : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(c => c.Services)
            .WithOne()
            .HasForeignKey("CarId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}