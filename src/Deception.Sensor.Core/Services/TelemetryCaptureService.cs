using Microsoft.AspNetCore.Http;
using Deception.Sensor.Core.Models;

namespace Deception.Sensor.Core.Services;

public class TelemetryCaptureService : ITelemetryCaptureService
{
    private RequestTelemetry? _lastCapturedTelemetry;

    public Task CaptureRequestTelemetryAsync(HttpContext context, string requestBody)
    {
        ArgumentNullException.ThrowIfNull(context);

        _lastCapturedTelemetry = new RequestTelemetry
        {
            Headers = RequestTelemetry.FromHeaders(context.Request.Headers),
            RequestBody = requestBody,
            Timestamp = DateTime.UtcNow,
            SourceIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            HttpMethod = context.Request.Method,
            Path = context.Request.Path.ToString()
        };

        return Task.CompletedTask;
    }

    public RequestTelemetry? GetLastCapturedTelemetry()
    {
        return _lastCapturedTelemetry;
    }
}
