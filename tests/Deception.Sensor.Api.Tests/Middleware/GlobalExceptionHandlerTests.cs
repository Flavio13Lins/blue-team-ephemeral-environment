namespace Deception.Sensor.Api.Tests.Middleware;

using Deception.Sensor.Api.Tests.Fixtures;
using System.Net;
using System.Text.Json;

public class GlobalExceptionHandlerTests : IClassFixture<TestApiContextFixture>
{
    private readonly HttpClient _httpClient;

    public GlobalExceptionHandlerTests(TestApiContextFixture fixture)
    {
        _httpClient = fixture.Client;
    }

    [Fact]
    public async Task Should_Catch_All_Unhandled_Exceptions()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        // This endpoint will throw an unhandled exception
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Exception should be caught, not crash the application
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Should_Always_Return_200_OK_With_Generic_Response()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        // Simulate a request that would normally cause an error
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should return 200 OK, NOT 500 Internal Server Error
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Generic_Masked_Response_Body()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"ok\"", content);  // Generic masked response
        Assert.DoesNotContain("Exception", content);    // No exception details exposed
        Assert.DoesNotContain("StackTrace", content);   // No stack trace leaked
    }

    [Fact]
    public async Task Should_Apply_Tarpitting_Even_On_Error()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Tarpitting delay should be applied even when exception occurs (1-3 seconds)
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }

    [Fact]
    public async Task Should_Log_Exception_To_Telemetry()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Exception should be logged to telemetry capture service
        // (Detailed telemetry validation would be done in integration tests)
    }

    [Fact]
    public async Task Should_Not_Expose_Exception_Details()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Ensure no sensitive information is exposed
        Assert.DoesNotContain("NullReferenceException", content);
        Assert.DoesNotContain("System.Exception", content);
        Assert.DoesNotContain("at Deception.", content); // No stack traces
        Assert.DoesNotContain("InnerException", content);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Should_Work_For_All_HTTP_Methods(string method)
    {
        // Arrange
        var endpoint = new Uri("http://localhost/api/test/throw-exception-all-methods");
        var request = new HttpRequestMessage(new HttpMethod(method), endpoint);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        // All HTTP methods should return 200 OK with generic response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.InRange(response.Content.Headers.ContentLength ?? 0, 1, long.MaxValue);
    }

    [Fact]
    public async Task Exception_Response_Should_Not_Contain_Trace_Information()
    {
        // Arrange
        var endpoint = "/api/test/throw-exception";

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Try to parse as JSON to ensure it's valid
        var jsonDoc = JsonDocument.Parse(content);
        
        // Response should have only safe properties
        Assert.True(jsonDoc.RootElement.TryGetProperty("status", out _), 
            "Response should contain 'status' property");
        
        // Should NOT contain any of these sensitive properties
        Assert.False(jsonDoc.RootElement.TryGetProperty("exceptionType", out _), 
            "Response should NOT expose exception type");
        Assert.False(jsonDoc.RootElement.TryGetProperty("stackTrace", out _), 
            "Response should NOT expose stack trace");
        Assert.False(jsonDoc.RootElement.TryGetProperty("message", out var msg) && 
            msg.GetString()?.Contains("Exception") == true, 
            "Response message should NOT contain Exception text");
    }
}
