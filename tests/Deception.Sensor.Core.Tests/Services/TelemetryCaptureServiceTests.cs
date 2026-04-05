namespace Deception.Sensor.Core.Tests.Services;

using Moq;
using Microsoft.AspNetCore.Http;

public class TelemetryCaptureServiceTests
{
    [Fact]
    public async Task Should_Capture_Request_Headers()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var headers = new HeaderDictionary
        {
            { "User-Agent", "Mozilla/5.0 (Attacker Bot)" },
            { "Authorization", "Bearer fake-token" }
        };
        
        mockContext.Setup(x => x.Request.Headers).Returns(headers);
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse("192.168.1.100"));
        mockContext.Setup(x => x.Request.Method).Returns("POST");
        mockContext.Setup(x => x.Request.Path).Returns("/api/auth/login");

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "");
        var telemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Headers);
        Assert.Contains("User-Agent", telemetry.Headers.Keys);
    }

    [Fact]
    public async Task Should_Capture_Request_Body_Payload()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var payload = "{\"username\":\"admin\",\"password\":\"admin123\"}";
        
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse("192.168.1.100"));
        mockContext.Setup(x => x.Request.Method).Returns("POST");
        mockContext.Setup(x => x.Request.Path).Returns("/api/auth/login");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, payload);
        var telemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Equal(payload, telemetry.RequestBody);
    }

    [Fact]
    public async Task Should_Capture_Request_Timestamp()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var beforeCapture = DateTime.UtcNow;
        
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse("192.168.1.100"));
        mockContext.Setup(x => x.Request.Method).Returns("GET");
        mockContext.Setup(x => x.Request.Path).Returns("/api/users/export");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "");
        var telemetry = service.GetLastCapturedTelemetry();
        var afterCapture = DateTime.UtcNow;

        // Assert
        Assert.NotNull(telemetry);
        Assert.True(telemetry.Timestamp >= beforeCapture && telemetry.Timestamp <= afterCapture);
    }

    [Fact]
    public async Task Should_Log_To_Telemetry_Storage()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse("10.0.0.1"));
        mockContext.Setup(x => x.Request.Method).Returns("POST");
        mockContext.Setup(x => x.Request.Path).Returns("/api/auth/login");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "payload1");
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "payload2");
        var lastTelemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(lastTelemetry);
        Assert.Equal("payload2", lastTelemetry.RequestBody); // Last one should be stored
    }

    [Fact]
    public async Task Should_Include_Source_IP_Address()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var attackerIp = "203.0.113.45";
        
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse(attackerIp));
        mockContext.Setup(x => x.Request.Method).Returns("GET");
        mockContext.Setup(x => x.Request.Path).Returns("/api/users/export");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "");
        var telemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Equal(attackerIp, telemetry.SourceIp);
    }

    [Fact]
    public async Task Should_Include_HTTP_Method_And_Path()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse("192.168.1.100"));
        mockContext.Setup(x => x.Request.Method).Returns("POST");
        mockContext.Setup(x => x.Request.Path).Returns("/api/auth/login");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, "");
        var telemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Equal("POST", telemetry.HttpMethod);
        Assert.Equal("/api/auth/login", telemetry.Path);
    }

    [Fact]
    public async Task Should_Log_Attacker_Ip_And_Payload()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var attackerIp = "192.168.1.100";
        var maliciousPayload = "{\"username\":\"' OR '1'='1\",\"password\":\"admin\"}";
        
        mockContext.Setup(x => x.Connection.RemoteIpAddress)
            .Returns(System.Net.IPAddress.Parse(attackerIp));
        mockContext.Setup(x => x.Request.Method).Returns("POST");
        mockContext.Setup(x => x.Request.Path).Returns("/api/auth/login");
        mockContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var service = new TelemetryCaptureService();

        // Act
        await service.CaptureRequestTelemetryAsync(mockContext.Object, maliciousPayload);
        var telemetry = service.GetLastCapturedTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Equal(attackerIp, telemetry.SourceIp);          // Attacker IP logged
        Assert.Equal(maliciousPayload, telemetry.RequestBody); // Payload logged
        Assert.NotNull(telemetry.Timestamp);                   // Timestamp recorded
    }
}
