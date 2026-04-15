using Microsoft.AspNetCore.Http;

namespace Deception.Sensor.Core.Models;

public class RequestTelemetry
{
    public Dictionary<string, string> Headers { get; init; } = new();
    public string RequestBody { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string SourceIp { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string AttackerIp { get; init; } = string.Empty;
    public string ProxyIp { get; init; } = string.Empty;

    public static Dictionary<string, string> FromHeaders(IHeaderDictionary headers)
    {
        return headers.ToDictionary(header => header.Key, header => header.Value.ToString());
    }
}
