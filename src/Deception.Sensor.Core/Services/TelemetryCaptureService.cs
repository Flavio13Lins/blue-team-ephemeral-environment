using Microsoft.AspNetCore.Http;
using Deception.Sensor.Core.Models;
using Microsoft.Extensions.Logging;

namespace Deception.Sensor.Core.Services;

public class TelemetryCaptureService : ITelemetryCaptureService
{
    private readonly ILogger<TelemetryCaptureService> _logger = null!;
    private RequestTelemetry? _lastCapturedTelemetry;

    public TelemetryCaptureService(ILogger<TelemetryCaptureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task CaptureRequestTelemetryAsync(HttpContext context, string requestBody)
    {
        ArgumentNullException.ThrowIfNull(context);

        var proxyIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        _lastCapturedTelemetry = new RequestTelemetry
        {
            Headers = RequestTelemetry.FromHeaders(context.Request.Headers),
            RequestBody = requestBody,
            Timestamp = DateTime.UtcNow,
            SourceIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            AttackerIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? proxyIp,
            ProxyIp = proxyIp,
            HttpMethod = context.Request.Method,
            Path = context.Request.Path.ToString()
        };

        _logger.LogInformation("Attack telemetry captured: {@Telemetry}", _lastCapturedTelemetry);

        return Task.CompletedTask;
    }

    public RequestTelemetry? GetLastCapturedTelemetry()
    {
        return _lastCapturedTelemetry;
    }
}
