using System.Text;

namespace Deception.Sensor.Api.Infrastructure;

public static class DeceptionHttpContextExtensions
{
    private const string CachedRequestBodyKey = "Deception.Sensor.Api.CachedRequestBody";

    public static async Task<string> ReadCachedRequestBodyAsync(this HttpContext context)
    {
        if (context.Items.TryGetValue(CachedRequestBodyKey, out var cachedBody) &&
            cachedBody is string body)
        {
            return body;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        context.Items[CachedRequestBodyKey] = requestBody;

        return requestBody;
    }
}
