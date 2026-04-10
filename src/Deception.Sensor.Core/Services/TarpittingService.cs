using System.Diagnostics;

namespace Deception.Sensor.Core.Services;

public class TarpittingService : ITarpittingService
{
    private const int MinDelayMs = 1000;
    private const int MaxDelayMs = 3000;

    public int GetRandomDelay()
    {
        return Random.Shared.Next(MinDelayMs, MaxDelayMs + 1);
    }

    public async Task ExecuteWithTarpittingAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var targetDelay = GetRandomDelay();
        var stopwatch = Stopwatch.StartNew();

        await action();

        await ApplyRemainingDelayAsync(stopwatch, targetDelay);
    }

    public async Task<T> ExecuteWithTarpittingAsync<T>(Func<Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var targetDelay = GetRandomDelay();
        var stopwatch = Stopwatch.StartNew();
        var result = await action();

        await ApplyRemainingDelayAsync(stopwatch, targetDelay);

        return result;
    }

    private static async Task ApplyRemainingDelayAsync(Stopwatch stopwatch, int targetDelay)
    {
        stopwatch.Stop();

        var remainingDelay = targetDelay - (int)stopwatch.ElapsedMilliseconds;
        if (remainingDelay > 0)
        {
            await Task.Delay(remainingDelay);
        }
    }
}
