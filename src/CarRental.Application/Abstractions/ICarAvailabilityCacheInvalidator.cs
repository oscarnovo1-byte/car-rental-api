namespace CarRental.Application.Abstractions;

public interface ICarAvailabilityCacheInvalidator
{
    void Invalidate();
}