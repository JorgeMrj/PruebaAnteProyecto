using System.Net.WebSockets;
using FunkoApi.Dto.Categories;
using FunkoApi.Handler.Categorias;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestFunko.unit.webSocket;

/// <summary>
/// Unit tests for CategoriaWebSocketHandler following FIRST principles.
/// These tests are self-explanatory, use mocks, and handle Guid IDs.
/// </summary>
[TestFixture]
public class CategoriaWebSocketHandlerTests
{
    private Mock<ILogger<CategoriaWebSocketHandler>> _mockLogger;
    private CategoriaWebSocketHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CategoriaWebSocketHandler>>();
        _handler = new CategoriaWebSocketHandler(_mockLogger.Object);
    }

    [Test]
    public void GetConnectionCount_WhenNoConnections_ShouldReturnZero()
    {
        // Arrange & Act
        var count = _handler.GetConnectionCount();

        // Assert
        Assert.That(count, Is.EqualTo(0), "Initially there should be no connections");
    }

    [Test]
    public async Task HandleConnectionAsync_WhenSocketIsClosed_ShouldAddAndRemoveConnection()
    {
        // Arrange
        var mockContext = new Mock<HttpContext>();
        var mockWebSocket = new Mock<WebSocket>();

        // Set up the socket to return "Close" immediately to exit the loop in the handler
        mockWebSocket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Closing"));
        
        mockWebSocket.Setup(s => s.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleConnectionAsync(mockContext.Object, mockWebSocket.Object);

        // Assert
        Assert.That(_handler.GetConnectionCount(), Is.EqualTo(0), "The connection should be removed after the socket closes");
        mockWebSocket.Verify(s => s.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task NotifyAsync_WhenClientsAreConnected_ShouldSendMessageToAllOpenSockets()
    {
        // Arrange
        var mockSocket1 = CreateMockWebSocket(WebSocketState.Open);
        var mockSocket2 = CreateMockWebSocket(WebSocketState.Open);
        
        var t1 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket1.Object);
        var t2 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket2.Object);

        await Task.Delay(50);

        var notification = new CategoriaNotificacion(
            CategoriaNotificationType.CREATED,
            Guid.NewGuid(),
            new CategoriaResponseDto(new Guid(), "Test Category"));

        // Act
        await _handler.NotifyAsync(notification);

        // Assert
        mockSocket1.Verify(s => s.SendAsync(
            It.IsAny<ArraySegment<byte>>(), 
            WebSocketMessageType.Text, 
            true, 
            It.IsAny<CancellationToken>()), Times.Once);

        mockSocket2.Verify(s => s.SendAsync(
            It.IsAny<ArraySegment<byte>>(), 
            WebSocketMessageType.Text, 
            true, 
            It.IsAny<CancellationToken>()), Times.Once);

        // Clean up
        CompleteMockWebSocketClose(mockSocket1);
        CompleteMockWebSocketClose(mockSocket2);
        await Task.WhenAll(t1, t2);
    }

    [Test]
    public async Task NotifyAsync_WhenSocketIsAborted_ShouldRemoveItFromConnections()
    {
        // Arrange
        var mockSocket = CreateMockWebSocket(WebSocketState.Aborted);
        var t1 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket.Object);
        
        await Task.Delay(50);
        Assert.That(_handler.GetConnectionCount(), Is.EqualTo(1));

        // Act
        await _handler.NotifyAsync(new CategoriaNotificacion("TEST", Guid.NewGuid(), null));

        // Assert
        Assert.That(_handler.GetConnectionCount(), Is.EqualTo(0), "The aborted socket should have been removed");
        
        // Clean up
        CompleteMockWebSocketClose(mockSocket);
        await t1;
    }

    [Test]
    public async Task NotifyAsync_WhenSendThrowsException_ShouldRemoveConnection()
    {
        // Arrange
        var mockSocket = CreateMockWebSocket(WebSocketState.Open);
        mockSocket.Setup(s => s.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network failure"));

        var t1 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket.Object);
        await Task.Delay(50);

        // Act
        await _handler.NotifyAsync(new CategoriaNotificacion("TEST", Guid.NewGuid(), null));

        // Assert
        Assert.That(_handler.GetConnectionCount(), Is.EqualTo(0), "Connection should be removed if sending fails");

        // Clean up
        CompleteMockWebSocketClose(mockSocket);
        await t1;
    }

    #region Helper Methods for WebSockets

    private Mock<WebSocket> CreateMockWebSocket(WebSocketState state)
    {
        var mock = new Mock<WebSocket>();
        mock.Setup(s => s.State).Returns(state);
        
        var tcs = new TaskCompletionSource<WebSocketReceiveResult>();
        mock.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        mock.As<IDisposable>().Setup(d => d.Dispose());
        _receiveTaskCompletions[mock] = tcs;

        return mock;
    }

    private readonly Dictionary<Mock<WebSocket>, TaskCompletionSource<WebSocketReceiveResult>> _receiveTaskCompletions = new();

    private void CompleteMockWebSocketClose(Mock<WebSocket> mock)
    {
        if (_receiveTaskCompletions.TryGetValue(mock, out var tcs))
        {
            tcs.SetResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Closing"));
        }
    }

    #endregion
}
