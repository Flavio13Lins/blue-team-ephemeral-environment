namespace Deception.Sensor.Api.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Microsoft.Extensions.Logging;
using Deception.Sensor.Core.Services;
using Microsoft.Extensions.DependencyInjection;

public class TestApiContextFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }
    public Mock<ILogger<Program>> MockLogger { get; }

    public TestApiContextFixture()
    {
        MockLogger = new Mock<ILogger<Program>>();

        // Create WebApplicationFactory that configures the application for testing
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove real logger and replace with mock
                    var loggerProvider = services.FirstOrDefault(sd => 
                        sd.ServiceType == typeof(ILogger<Program>));
                    if (loggerProvider != null)
                    {
                        services.Remove(loggerProvider);
                    }

                    services.AddScoped(sp => MockLogger.Object);

                    // Register services for dependency injection in tests
                    // Ensure ITarpittingService and ITelemetryCaptureService are available
                    if (!services.Any(x => x.ServiceType == typeof(ITarpittingService)))
                    {
                        services.AddScoped<ITarpittingService, TarpittingService>();
                    }

                    if (!services.Any(x => x.ServiceType == typeof(ITelemetryCaptureService)))
                    {
                        services.AddScoped<ITelemetryCaptureService, TelemetryCaptureService>();
                    }
                });
            });

        // Create HTTP client that will be reused across all tests in this fixture
        Client = Factory.CreateClient();
    }

    /// <summary>
    /// Disposes the factory and cleans up resources
    /// </summary>
    public void Dispose()
    {
        Factory?.Dispose();
        Client?.Dispose();
    }
}
