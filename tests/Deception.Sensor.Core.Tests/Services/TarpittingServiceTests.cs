namespace Deception.Sensor.Core.Tests.Services;

public class TarpittingServiceTests
{
    [Fact]
    public void Delay_Should_Not_Exceed_3_Seconds()
    {
        // Arrange
        var service = new TarpittingService();
        const int maxDelay = 3000;

        // Act
        var delay = service.GetRandomDelay();

        // Assert
        Assert.True(delay <= maxDelay, $"Delay {delay}ms exceeded maximum {maxDelay}ms");
    }

    [Fact]
    public void Delay_Should_Be_At_Least_1_Second()
    {
        // Arrange
        var service = new TarpittingService();
        const int minDelay = 1000;

        // Act
        var delay = service.GetRandomDelay();

        // Assert
        Assert.True(delay >= minDelay, $"Delay {delay}ms is less than minimum {minDelay}ms");
    }

    [Fact]
    public async Task Can_Execute_Action_With_Tarpitting_Applied()
    {
        // Arrange
        var service = new TarpittingService();
        var actionExecuted = false;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await service.ExecuteWithTarpittingAsync(async () =>
        {
            actionExecuted = true;
            await Task.Delay(0);
        });
        stopwatch.Stop();

        // Assert
        Assert.True(actionExecuted, "Action should have been executed");
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }

    [Fact]
    public async Task Should_Compensate_Already_Elapsed_Time()
    {
        // Arrange
        var service = new TarpittingService();
        const int actionDuration = 500; // Action takes 500ms
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await service.ExecuteWithTarpittingAsync(async () =>
        {
            await Task.Delay(actionDuration);
        });
        stopwatch.Stop();

        // Assert
        // Total time should still be between 1-3 seconds (compensating the action duration)
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }

    [Fact]
    public async Task ExecuteWithTarpittingAsync_Generic_Should_Return_Result_And_Apply_Delay()
    {
        // Arrange
        var service = new TarpittingService();
        var expectedResult = "test-result";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await service.ExecuteWithTarpittingAsync(async () =>
        {
            await Task.Delay(0);
            return expectedResult;
        });
        stopwatch.Stop();

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }
}
