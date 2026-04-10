namespace Deception.Sensor.Core.Services;

public interface ITarpittingService
{
    int GetRandomDelay();
    Task ExecuteWithTarpittingAsync(Func<Task> action);
    Task<T> ExecuteWithTarpittingAsync<T>(Func<Task<T>> action);
}
