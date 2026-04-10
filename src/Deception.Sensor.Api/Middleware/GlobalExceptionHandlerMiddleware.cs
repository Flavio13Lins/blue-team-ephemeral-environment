using Deception.Sensor.Api.Infrastructure;
using Deception.Sensor.Core.Services;

namespace Deception.Sensor.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITarpittingService tarpittingService,
        ITelemetryCaptureService telemetryCaptureService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            var requestBody = await context.ReadCachedRequestBodyAsync();
            await telemetryCaptureService.CaptureRequestTelemetryAsync(context, requestBody);

            context.Response.Clear();

            await tarpittingService.ExecuteWithTarpittingAsync(
                () => DeceptionHttpResponses.WriteGenericSuccessAsync(context));
        }
    }
}
