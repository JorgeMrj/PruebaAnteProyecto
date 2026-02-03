using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FunkoApi.Dto.Categories;

namespace FunkoApi.Handler.Categorias;

/// <summary>
/// Tipos de notificación para eventos de Categorias.
/// </summary>
public class CategoriaNotificationType
{
    /// <summary>
    /// Notificación de Categoria creado.
    /// </summary>
    public const string CREATED = "CATEGORIA_CREADO";

    /// <summary>
    /// Notificación de Categoria actualizado.
    /// </summary>
    public const string UPDATED = "CATEGORIA_ACTUALIZADO";

    /// <summary>
    /// Notificación de Categoria eliminado.
    /// </summary>
    public const string DELETED = "CATEGORIA_ELIMINADO";
}

/// <summary>
/// Datos de notificación para eventos de Categorias.
/// </summary>
/// <param name="Tipo">Tipo de notificación (CREATED, UPDATED, DELETED).</param>
/// <param name="CategoriaId">Identificador del Categoria.</param>
/// <param name="Categoria">Datos del Categoria (null para DELETED).</param>
public record CategoriaNotificacion(
    string Tipo,
    Guid CategoriaId,
    CategoriaResponseDto? Categoria
);

/// <summary>
/// Handler de WebSocket para gestionar conexiones de notificaciones de Categorias.
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
/// <code>ws://localhost:5000/ws/Categorias</code>
/// 
/// <para><b>Ejemplo de conexión desde cliente JavaScript:</b></para>
/// <code>
/// // Sin autenticación requerida
/// const ws = new WebSocket('ws://localhost:5000/ws/Categorias');
///
/// ws.onmessage = (event) => {
///     const data = JSON.parse(event.data);
///     console.log('Notificación de Categoria:', data);
/// };
/// </code>
/// 
/// <para><b>Casos de uso:</b></para>
/// <list type="bullet">
///   <item><description>Dashboards públicos que muestran nuevos Categorias.</description></item>
///   <item><description>Actualización de catálogos en tiempo real.</description></item>
///   <item><description>Sistemas de inventario que monitorean cambios.</description></item>
/// </list>
/// 
/// <para><b>Ejemplo de respuesta de notificación:</b></para>
/// <code>
/// {
///   "entity": "Categorias",
///   "type": "Categoria_CREADO",
///   "CategoriaId": 123,
///   "Categoria": {
///     "id": 123,
///     "nombre": "Nuevo Categoria",
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
///     <term>Categoria_CREADO</term>
///     <description>Se creó un nuevo Categoria. Incluye datos del Categoria.</description>
///   </item>
///   <item>
///     <term>Categoria_ACTUALIZADO</term>
///     <description>Se actualizó un Categoria. Incluye datos actualizados.</description>
///   </item>
///   <item>
///     <term>Categoria_ELIMINADO</term>
///     <description>Se eliminó un Categoria. Categoria es null, solo envía CategoriaId.</description>
///   </item>
/// </list>
/// </remarks>
public class CategoriaWebSocketHandler(ILogger<CategoriaWebSocketHandler> logger)
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Maneja una nueva conexión WebSocket para Categorias.
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

        logger.LogInformation("Conexión WebSocket establecida para Categorias: {ConnectionId}", connectionId);

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
            logger.LogError(ex, "Error en conexión WebSocket para Categorias: {ConnectionId}", connectionId);
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);
            logger.LogInformation("Conexión WebSocket cerrada para Categorias: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Notifica a todos los clientes conectados un evento de Categoria.
    /// </summary>
    /// <param name="notificacion">Datos de la notificación.</param>
    /// <returns>Tarea asíncrona de la notificación.</returns>
    /// <remarks>
    /// <para><b>Ejemplo de uso:</b></para>
    /// <code>
    /// // Notificar que se creó un nuevo Categoria
    /// await NotifyAsync(new CategoriaNotificacion(
    ///     CategoriaNotificationType.CREATED,
    ///     123,
    ///     CategoriaDto  // Datos del Categoria
    /// ));
    /// // TODOS los clientes conectados recibirán esta notificación
    ///
    /// // Notificar que se eliminó un Categoria
    /// await NotifyAsync(new CategoriaNotificacion(
    ///     CategoriaNotificationType.DELETED,
    ///     123,
    ///     null  // No hay datos del Categoria eliminado
    /// ));
    /// </code>
    /// </remarks>
    public async Task NotifyAsync(CategoriaNotificacion notificacion)
    {
        var wrapper = new
        {
            entity = "categoria",
            type = notificacion.Tipo,
            categoriaId = notificacion.CategoriaId,
            categoria = notificacion.Categoria,
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
            logger.LogDebug("No hay clientes WebSocket conectados para Categorias, omitiendo notificación");
            return;
        }

        var json = JsonSerializer.Serialize(notification, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        logger.LogInformation(
            "Broadcasting notificación de Categoria a {Count} clientes",
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
                logger.LogWarning(ex, "Error al enviar notificación a la conexión: {ConnectionId}", kvp.Key);
                disconnectedConnections.Add(kvp.Key);
            }
        }

        foreach (var connectionId in disconnectedConnections)
        {
            _connections.TryRemove(connectionId, out _);
            logger.LogDebug("Eliminado cliente WebSocket de Categoria desconectado: {ConnectionId}", connectionId);
        }
    }

    #endregion
}