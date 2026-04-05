namespace Deception.Sensor.Api.Tests.Endpoints;

using Deception.Sensor.Api.Tests.Fixtures;
using System.Net;
using System.Text;
using System.Text.Json;

public class AuthLoginEndpointTests : IClassFixture<TestApiContextFixture>
{
    private readonly HttpClient _httpClient;

    public AuthLoginEndpointTests(TestApiContextFixture fixture)
    {
        _httpClient = fixture.Client;
    }

    [Fact]
    public async Task Should_Accept_Login_Request_And_Return_200_OK()
    {
        // Arrange
        var loginPayload = new { username = "admin", password = "admin" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Generic_Success_Response()
    {
        // Arrange
        var loginPayload = new { username = "admin", password = "password123" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"ok\"", responseBody);  // Generic masked response
    }

    [Fact]
    public async Task Should_Apply_Tarpitting_Delay_On_Response()
    {
        // Arrange
        var loginPayload = new { username = "admin", password = "admin" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Request should be delayed by 1-3 seconds (tarpitting)
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }

    [Fact]
    public async Task Should_Capture_Login_Credentials_Attempt()
    {
        // Arrange
        var loginPayload = new { username = "testuser", password = "testpass" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Credentials should be captured by telemetry (detailed validation in integration)
    }

    [Fact]
    public async Task Should_Accept_Any_Credentials()
    {
        // Arrange - Try with random credentials (bot/attacker trying different passwords)
        var randomUsername = Guid.NewGuid().ToString();
        var randomPassword = Guid.NewGuid().ToString();

        var loginPayload = new { username = randomUsername, password = randomPassword };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Assert
        // Endpoint should accept ANY credentials (engaging bots/attackers)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Not_Validate_Input()
    {
        // Arrange - Send empty/null credentials
        var loginPayload = new { username = "", password = "" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Assert
        // Should accept empty credentials (no validation = vulnerable = engaging)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("' OR '1'='1", "admin")]           // Classic SQL injection
    [InlineData("admin", "' OR '1'='1'")]          // SQL injection in password
    [InlineData("admin'; DROP TABLE users; --", "admin")]  // Destructive injection
    [InlineData("<script>alert('xss')</script>", "admin")]  // XSS payload
    public async Task Should_Accept_Malicious_Credentials(string username, string password)
    {
        // Arrange
        var loginPayload = new { username, password };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync("/api/auth/login", content);

        // Assert
        // Should accept malicious payloads without validation (engaging attackers)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Generic_Response_Regardless_Of_Input()
    {
        // Arrange - Send valid credentials
        var validPayload = new { username = "admin", password = "admin" };
        var validContent = new StringContent(
            JsonSerializer.Serialize(validPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var validResponse = await _httpClient.PostAsync("/api/auth/login", validContent);
        var validBody = await validResponse.Content.ReadAsStringAsync();

        // Arrange - Send malicious credentials
        var maliciousPayload = new { username = "' OR '1'='1", password = "admin" };
        var maliciousContent = new StringContent(
            JsonSerializer.Serialize(maliciousPayload),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var maliciousResponse = await _httpClient.PostAsync("/api/auth/login", maliciousContent);
        var maliciousBody = await maliciousResponse.Content.ReadAsStringAsync();

        // Assert - Both should return the same generic response
        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, maliciousResponse.StatusCode);
        // Response should be identical regardless of input
        Assert.Equal(validBody, maliciousBody);
    }
}
