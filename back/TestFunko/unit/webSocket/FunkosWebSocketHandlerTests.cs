using System.Net.WebSockets;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Handler.Funkos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestFunko.unit.webSocket;

/// <summary>
/// Unit tests for FunkosWebSocketHandler following FIRST principles.
/// These tests are self-explanatory and use mocks for external dependencies.
/// </summary>
[TestFixture]
public class FunkosWebSocketHandlerTests
{
    private Mock<ILogger<FunkosWebSocketHandler>> _mockLogger;
    private FunkosWebSocketHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<FunkosWebSocketHandler>>();
        _handler = new FunkosWebSocketHandler(_mockLogger.Object);
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
        // We run the connection handler. Since it's a loop that depends on the socket state,
        // we mocked it to "close" immediately.
        await _handler.HandleConnectionAsync(mockContext.Object, mockWebSocket.Object);

        // Assert
        Assert.That(_handler.GetConnectionCount(), Is.EqualTo(0), "The connection should be removed after the socket closes");
        
        // Success is also implicit if no exceptions were thrown
        mockWebSocket.Verify(s => s.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task NotifyAsync_WhenClientsAreConnected_ShouldSendMessageToAllOpenSockets()
    {
        // Arrange
        // We need to simulate connected clients.
        // Since the connections dictionary is private, we use HandleConnectionAsync with a fake blocking receive.
        
        var mockSocket1 = CreateMockWebSocket(WebSocketState.Open);
        var mockSocket2 = CreateMockWebSocket(WebSocketState.Open);
        
        // Use Task.Run to simulate background connections that stay open for a bit
        var t1 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket1.Object);
        var t2 = _handler.HandleConnectionAsync(new Mock<HttpContext>().Object, mockSocket2.Object);

        // Give a small time for the connections to be added to the internal dictionary
        await Task.Delay(50); 

        var notification = new FunkoNotificacion(
            FunkoNotificationType.Created,
            1L,
            new FunkoResponseDto(1L,"test funko",1.0,"dc","default.png") 
        );

        // Act
        await _handler.NotifyAsync(notification);

        // Assert
        // Verify that both sockets received a message
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

        // Clean up: stop the background handlers
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
        // Sending a notification should detect the aborted socket and remove it
        await _handler.NotifyAsync(new FunkoNotificacion("TEST", 0, null));

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
        await _handler.NotifyAsync(new FunkoNotificacion("TEST", 0, null));

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
        
        // A tcs to control when the ReceiveAsync returns a "Close" signal
        var tcs = new TaskCompletionSource<WebSocketReceiveResult>();
        mock.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Store the TCS in the mock using a callback or similar if needed, 
        // but for these tests we can just keep the reference if we needed to complete it.
        // Actually, let's use a simpler way with a shared state if needed.
        mock.As<IDisposable>().Setup(d => d.Dispose());
        
        // Attach the TCS to the mock's Tag-like behavior via a setup if we wanted, 
        // but here we'll just handle it in the test scope.
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
