namespace Deception.Sensor.Api.Infrastructure;

public static class DeceptionHttpResponses
{
    public const string GenericSuccessJson = "{\"status\":\"ok\"}";

    public static async Task WriteGenericSuccessAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(GenericSuccessJson);
    }
}
