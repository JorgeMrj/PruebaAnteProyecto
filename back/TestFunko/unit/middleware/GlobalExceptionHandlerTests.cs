using System.Text.Json;
using FunkoApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandler following FIRST principles.
/// Verifies that specific exceptions are mapped to the correct HTTP status codes and JSON responses.
/// </summary>
[TestFixture]
public class GlobalExceptionHandlerTests
{
    private Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private DefaultHttpContext _context;
    private MemoryStream _responseStream;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _context = new DefaultHttpContext();
        _responseStream = new MemoryStream();
        _context.Response.Body = _responseStream;
    }

    [TearDown]
    public void TearDown()
    {
        _responseStream.Dispose();
    }

    [Test]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException());

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        var response = await GetResponseAsJson();
        Assert.That(response.GetProperty("errorType").GetString(), Is.EqualTo("UnauthorizedError"));
        Assert.That(response.GetProperty("message").GetString(), Is.EqualTo("No autorizado"));
    }

    [Test]
    public async Task InvokeAsync_WhenArgumentException_ShouldReturn400()
    {
        // Arrange
        var message = "Invalid argument";
        var middleware = CreateMiddleware(_ => throw new ArgumentException(message));

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        var response = await GetResponseAsJson();
        Assert.That(response.GetProperty("errorType").GetString(), Is.EqualTo("ValidationError"));
        Assert.That(response.GetProperty("message").GetString(), Is.EqualTo(message));
    }

    [Test]
    public async Task InvokeAsync_WhenDbUpdateException_ShouldReturn409()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new DbUpdateException());

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        var response = await GetResponseAsJson();
        Assert.That(response.GetProperty("errorType").GetString(), Is.EqualTo("ConflictError"));
    }

    [Test]
    public async Task InvokeAsync_WhenTimeoutException_ShouldReturn408()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new TimeoutException());

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status408RequestTimeout));
        var response = await GetResponseAsJson();
        Assert.That(response.GetProperty("errorType").GetString(), Is.EqualTo("InternalError"));
        Assert.That(response.GetProperty("message").GetString(), Is.EqualTo("Tiempo de espera agotado"));
    }

    [Test]
    public async Task InvokeAsync_WhenGenericException_ShouldReturn500()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new Exception("Boom!"));

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        var response = await GetResponseAsJson();
        Assert.That(response.GetProperty("errorType").GetString(), Is.EqualTo("InternalError"));
        Assert.That(response.GetProperty("message").GetString(), Is.EqualTo("Ha ocurrido un error interno"));
    }

    [Test]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new GlobalExceptionHandler(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(nextCalled, Is.True, "The next delegate should be called if no exception occurs");
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    #region Helper Methods

    private GlobalExceptionHandler CreateMiddleware(RequestDelegate nextDelegate)
    {
        return new GlobalExceptionHandler(nextDelegate, _mockLogger.Object);
    }

    private async Task<JsonElement> GetResponseAsJson()
    {
        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    #endregion
}
