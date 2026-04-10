using Microsoft.AspNetCore.Http;
using Deception.Sensor.Core.Models;

namespace Deception.Sensor.Core.Services;

public interface ITelemetryCaptureService
{
    Task CaptureRequestTelemetryAsync(HttpContext context, string requestBody);
    RequestTelemetry? GetLastCapturedTelemetry();
}
