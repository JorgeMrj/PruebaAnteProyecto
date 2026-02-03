using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FunkoApi.Dto.Funkasos;

namespace FunkoApi.Handler.Funkos;

/// <summary>
/// Tipos de notificación para eventos de funkos.
/// </summary>
public class FunkoNotificationType
{
    /// <summary>
    /// Notificación de funko creado.
    /// </summary>
    public const string Created = "FUNKO_CREADO";

    /// <summary>
    /// Notificación de funko actualizado.
    /// </summary>
    public const string Updated = "FUNKO_ACTUALIZADO";

    /// <summary>
    /// Notificación de funko eliminado.
    /// </summary>
    public const string Deleted = "FUNKO_ELIMINADO";
}

/// <summary>
/// Datos de notificación para eventos de funkos.
/// </summary>
/// <param name="Tipo">Tipo de notificación (CREATED, UPDATED, DELETED).</param>
/// <param name="FunkoId">Identificador del funko.</param>
/// <param name="Funko">Datos del funko (null para DELETED).</param>
public record FunkoNotificacion(
    string Tipo,
    long FunkoId,
    FunkoResponseDto? Funko
);

/// <summary>
/// Handler de WebSocket para gestionar conexiones de notificaciones de funkos.
/// </summary>
/// <remarks>
/// <para><b>Características:</b></para>
/// <list type="bullet">
///   <item><description>Notificaciones de broadcast a TODOS los clientes conectados.</description></item>
///   <item><description>No requiere autenticación (público).</description></item>
///   <item><description>Ideal para dashboards públicos y catálogos en tiempo real.</description></item>
/// </list>
/// 
/// <para><b>EndPoint de conexión:</b></para>
/// <code>ws://localhost:5000/ws/funkos</code>
/// 
/// <para><b>Ejemplo de conexión desde cliente JavaScript:</b></para>
/// <code>
/// // Sin autenticación requerida
/// const ws = new WebSocket('ws://localhost:5000/ws/funkos');
///
/// ws.onmessage = (event) => {
///     const data = JSON.parse(event.data);
///     console.log('Notificación de funko:', data);
/// };
/// </code>
/// 
/// <para><b>Casos de uso:</b></para>
/// <list type="bullet">
///   <item><description>Dashboards públicos que muestran nuevos funkos.</description></item>
///   <item><description>Actualización de catálogos en tiempo real.</description></item>
///   <item><description>Sistemas de inventario que monitorean cambios.</description></item>
/// </list>
/// 
/// <para><b>Ejemplo de respuesta de notificación:</b></para>
/// <code>
/// {
///   "entity": "funkos",
///   "type": "funko_CREADO",
///   "funkoId": 123,
///   "funko": {
///     "id": 123,
///     "nombre": "Nuevo funko",
///     "precio": 99.99,
///     "stock": 50
///   },
///   "timestamp": "2025-01-18T10:30:00Z"
/// }
/// </code>
/// 
/// <para><b>Tipos de eventos:</b></para>
/// <list type="table">
///   <item>
///     <term>funko_CREADO</term>
///     <description>Se creó un nuevo funko. Incluye datos del funko.</description>
///   </item>
///   <item>
///     <term>funko_ACTUALIZADO</term>
///     <description>Se actualizó un funko. Incluye datos actualizados.</description>
///   </item>
///   <item>
///     <term>funko_ELIMINADO</term>
///     <description>Se eliminó un funko. funko es null, solo envía funkoId.</description>
///   </item>
/// </list>
/// </remarks>
public class FunkosWebSocketHandler(ILogger<FunkosWebSocketHandler> logger)
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<FunkosWebSocketHandler> _logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Maneja una nueva conexión WebSocket para funkos.
    /// </summary>
    /// <param name="context">Contexto HTTP de la conexión.</param>
    /// <param name="webSocket">Instancia del WebSocket.</param>
    /// <returns>Tarea asíncrona representando la conexión.</returns>
    /// <remarks>
    /// <para><b>Proceso de conexión:</b></para>
    /// <list type="number">
    ///   <item><description>El cliente se conecta sin necesidad de autenticación.</description></item>
    ///   <item><description>Se genera un connectionId único para la sesión.</description></item>
    ///   <item><description>La conexión se almacena en el diccionario de conexiones.</description></item>
    ///   <item><description>Cuando se cierra la conexión, se elimina del diccionario.</description></item>
    /// </list>
    /// </remarks>
    public async Task HandleConnectionAsync(HttpContext context, WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);

        _logger.LogInformation("Conexión WebSocket establecida para funkos: {ConnectionId}", connectionId);

        try
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);
            }

            await webSocket.CloseAsync(
                result.CloseStatus.Value,
                result.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en conexión WebSocket para funkos: {ConnectionId}", connectionId);
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);
            _logger.LogInformation("Conexión WebSocket cerrada para funkos: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Notifica a todos los clientes conectados un evento de funko.
    /// </summary>
    /// <param name="notificacion">Datos de la notificación.</param>
    /// <returns>Tarea asíncrona de la notificación.</returns>
    /// <remarks>
    /// <para><b>Ejemplo de uso:</b></para>
    /// <code>
    /// // Notificar que se creó un nuevo funko
    /// await NotifyAsync(new funkoNotificacion(
    ///     funkoNotificationType.CREATED,
    ///     123,
    ///     funkoDto  // Datos del funko
    /// ));
    /// // TODOS los clientes conectados recibirán esta notificación
    ///
    /// // Notificar que se eliminó un funko
    /// await NotifyAsync(new funkoNotificacion(
    ///     funkoNotificationType.DELETED,
    ///     123,
    ///     null  // No hay datos del funko eliminado
    /// ));
    /// </code>
    /// </remarks>
    public async Task NotifyAsync(FunkoNotificacion notificacion)
    {
        var wrapper = new
        {
            entity = "funkos",
            type = notificacion.Tipo,
            funkoId = notificacion.FunkoId,
            funko = notificacion.Funko,
            timestamp = DateTime.UtcNow
        };

        await BroadcastNotificationAsync(wrapper);
    }

    /// <summary>
    /// Obtiene el número de conexiones activas.
    /// </summary>
    /// <returns>Número de conexiones activas.</returns>
    public int GetConnectionCount() => _connections.Count;

    #region Métodos Privados

    /// <summary>
    /// Envía una notificación a todos los clientes WebSocket conectados.
    /// </summary>
    /// <param name="notification">Notificación a broadcast.</param>
    /// <returns>Tarea asíncrona del broadcast.</returns>
    private async Task BroadcastNotificationAsync<T>(T notification)
    {
        if (_connections.IsEmpty)
        {
            _logger.LogDebug("No hay clientes WebSocket conectados para funkos, omitiendo notificación");
            return;
        }

        var json = JsonSerializer.Serialize(notification, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        _logger.LogInformation(
            "Broadcasting notificación de funko a {Count} clientes",
            _connections.Count);

        var disconnectedConnections = new List<string>();

        foreach (var kvp in _connections)
        {
            try
            {
                if (kvp.Value.State == WebSocketState.Open)
                {
                    await kvp.Value.SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        CancellationToken.None);
                }
                else
                {
                    disconnectedConnections.Add(kvp.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al enviar notificación a la conexión: {ConnectionId}", kvp.Key);
                disconnectedConnections.Add(kvp.Key);
            }
        }

        foreach (var connectionId in disconnectedConnections)
        {
            _connections.TryRemove(connectionId, out _);
            _logger.LogDebug("Eliminado cliente WebSocket de funko desconectado: {ConnectionId}", connectionId);
        }
    }

    #endregion
}