using Deception.Sensor.Api.Infrastructure;
using Deception.Sensor.Api.Middleware;
using Deception.Sensor.Core.Services;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITarpittingService, TarpittingService>();
builder.Services.AddScoped<ITelemetryCaptureService, TelemetryCaptureService>();


Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: "/app/logs/deception.log",
        formatter: new CompactJsonFormatter(),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10_000_000, // 10MB por arquivo
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapPost("/api/auth/login", HandleHoneyRequestAsync);
app.MapGet("/api/users/export", HandleHoneyRequestAsync);

app.MapGet("/api/test/throw-exception", () =>
{
    throw new InvalidOperationException("Simulated exception for deception testing.");
});

app.MapMethods("/api/test/throw-exception-all-methods", ["GET", "POST", "PUT", "DELETE"], () =>
{
    throw new InvalidOperationException("Simulated exception for deception testing.");
});

app.Run();

static async Task<IResult> HandleHoneyRequestAsync(
    HttpContext context,
    ITelemetryCaptureService telemetryCaptureService,
    ITarpittingService tarpittingService)
{
    var requestBody = await context.ReadCachedRequestBodyAsync();
    await telemetryCaptureService.CaptureRequestTelemetryAsync(context, requestBody);

    await tarpittingService.ExecuteWithTarpittingAsync(() => Task.CompletedTask);

    return Results.Text(DeceptionHttpResponses.GenericSuccessJson, "application/json");
}

public partial class Program;
