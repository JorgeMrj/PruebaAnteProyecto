using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FunkoApi.Handler.Funkos;

/// <summary>
/// Administrador centralizado de conexiones WebSocket para notificaciones en tiempo real.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase proporciona funcionalidades para gestionar conexiones WebSocket de múltiples clientes,
/// permitiendo comunicación bidireccional en tiempo real para notificaciones de cambios en productos Funko.
/// </para>
/// <para>
/// Características principales:
/// </para>
/// <list type="bullet">
/// <item><description>Gestión thread-safe de conexiones usando colecciones concurrentes</description></item>
/// <item><description>Soporte para mensajería uno-a-uno (unicast)</description></item>
/// <item><description>Soporte para mensajería uno-a-todos (broadcast)</description></item>
/// <item><description>Soporte para grupos/canales (envío selectivo a múltiples clientes)</description></item>
/// <item><description>Validación automática del estado de conexiones antes de enviar mensajes</description></item>
/// </list>
/// <para>
/// <strong>Thread-Safety:</strong> Todas las operaciones son thread-safe gracias al uso de
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </para>
/// </remarks>
/// <example>
/// Uso típico en un endpoint WebSocket:
/// <code>
/// app.UseWebSockets();
/// 
/// app.Map("/ws/funkos", async context =>
/// {
///     if (context.WebSockets.IsWebSocketRequest)
///     {
///         var webSocket = await context.WebSockets.AcceptWebSocketAsync();
///         var connectionId = connectionManager.AddConnection(webSocket);
///         
///         try
///         {
///             await HandleWebSocketConnection(webSocket, connectionId);
///         }
///         finally
///         {
///             connectionManager.RemoveConnection(connectionId);
///         }
///     }
/// });
/// </code>
/// 
/// Enviar notificación cuando se crea un Funko:
/// <code>
/// var message = JsonSerializer.Serialize(new WebSocketMessage
/// {
///     Type = "FUNKO_CREATED",
///     Payload = JsonSerializer.Serialize(funkoDto)
/// });
/// 
/// await connectionManager.BroadcastAsync(message);
/// </code>
/// </example>
public class WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
{
    /// <summary>
    /// Diccionario thread-safe que almacena todas las conexiones WebSocket activas indexadas por su ID único.
    /// </summary>
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    
    /// <summary>
    /// Diccionario thread-safe que almacena grupos/canales de conexiones.
    /// La clave es el nombre del grupo y el valor es un conjunto de IDs de conexión.
    /// </summary>
    /// <remarks>
    /// Permite agrupar conexiones para enviar mensajes selectivos (por ejemplo, solo a usuarios suscritos a una categoría específica).
    /// </remarks>
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    /// <summary>
    /// Agrega una nueva conexión WebSocket al administrador.
    /// </summary>
    /// <param name="webSocket">La instancia de <see cref="WebSocket"/> a agregar.</param>
    /// <returns>
    /// Un identificador único (GUID) asignado a la conexión que puede usarse para operaciones posteriores.
    /// </returns>
    /// <remarks>
    /// Este método es thread-safe y puede ser llamado desde múltiples hilos concurrentemente.
    /// El ID generado debe almacenarse para poder enviar mensajes específicos o eliminar la conexión posteriormente.
    /// </remarks>
    /// <example>
    /// <code>
    /// var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    /// var connectionId = connectionManager.AddConnection(webSocket);
    /// Console.WriteLine($"Nueva conexión: {connectionId}");
    /// </code>
    /// </example>
    public string AddConnection(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);
        logger.LogInformation("Conexión agregada: {ConnectionId}. Total: {Count}", 
            connectionId, _connections.Count);
        return connectionId;
    }

    /// <summary>
    /// Elimina una conexión WebSocket del administrador.
    /// </summary>
    /// <param name="connectionId">El identificador único de la conexión a eliminar.</param>
    /// <remarks>
    /// <para>
    /// Este método también elimina la conexión de todos los grupos a los que pertenecía.
    /// Es thread-safe y puede ser llamado múltiples veces con el mismo ID sin efectos secundarios.
    /// </para>
    /// <para>
    /// <strong>Importante:</strong> Este método NO cierra el WebSocket subyacente.
    /// El llamador es responsable de cerrar la conexión WebSocket antes o después de llamar a este método.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await webSocket.CloseAsync(
    ///         WebSocketCloseStatus.NormalClosure, 
    ///         "Conexión cerrada", 
    ///         CancellationToken.None);
    /// }
    /// finally
    /// {
    ///     connectionManager.RemoveConnection(connectionId);
    /// }
    /// </code>
    /// </example>
    public void RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var webSocket))
        {
            // Eliminar la conexión de todos los grupos
            foreach (var kvp in _userConnections)
            {
                kvp.Value.Remove(connectionId);
            }
            
            logger.LogInformation("Conexión eliminada: {ConnectionId}. Total: {Count}", 
                connectionId, _connections.Count);
        }
    }

    /// <summary>
    /// Obtiene una conexión WebSocket por su identificador.
    /// </summary>
    /// <param name="connectionId">El identificador único de la conexión.</param>
    /// <returns>
    /// La instancia de <see cref="WebSocket"/> si existe, de lo contrario <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Este método es útil para inspeccionar el estado de una conexión específica antes de realizar operaciones.
    /// </remarks>
    /// <example>
    /// <code>
    /// var webSocket = connectionManager.GetConnection(connectionId);
    /// if (webSocket?.State == WebSocketState.Open)
    /// {
    ///     // La conexión está activa
    /// }
    /// </code>
    /// </example>
    public WebSocket? GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var webSocket);
        return webSocket;
    }

    /// <summary>
    /// Envía un mensaje de texto a una conexión WebSocket específica.
    /// </summary>
    /// <param name="connectionId">El identificador único de la conexión destino.</param>
    /// <param name="message">El mensaje de texto a enviar (generalmente JSON serializado).</param>
    /// <returns>Una tarea que representa la operación asíncrona de envío.</returns>
    /// <remarks>
    /// <para>
    /// Este método verifica automáticamente que la conexión exista y esté en estado Open antes de enviar.
    /// Si la conexión no existe o no está abierta, el método no realiza ninguna acción y no lanza excepciones.
    /// </para>
    /// <para>
    /// El mensaje se codifica en UTF-8 y se envía como un mensaje de texto WebSocket completo (fin = true).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var message = JsonSerializer.Serialize(new 
    /// { 
    ///     type = "NOTIFICATION", 
    ///     data = "Nuevo Funko agregado" 
    /// });
    /// 
    /// await connectionManager.SendMessageAsync(connectionId, message);
    /// </code>
    /// </example>
    /// <exception cref="WebSocketException">
    /// Puede lanzarse si ocurre un error durante el envío del mensaje (conexión perdida, timeout, etc.).
    /// </exception>
    public async Task SendMessageAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var webSocket) && 
            webSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }

    /// <summary>
    /// Envía un mensaje a todas las conexiones WebSocket activas (broadcast).
    /// </summary>
    /// <param name="message">El mensaje de texto a enviar a todos los clientes conectados.</param>
    /// <returns>Una tarea que representa la operación asíncrona de broadcast.</returns>
    /// <remarks>
    /// <para>
    /// Este método filtra automáticamente solo las conexiones en estado Open antes de enviar.
    /// Los mensajes se envían secuencialmente a cada conexión.
    /// </para>
    /// <para>
    /// <strong>Rendimiento:</strong> Si hay muchas conexiones activas, considere usar
    /// <see cref="Task.WhenAll"/> para envíos paralelos, aunque esto puede consumir más recursos.
    /// </para>
    /// <para>
    /// Si algún envío falla, el error se propagará y detendrá el broadcast a las conexiones restantes.
    /// Considere agregar manejo de errores si requiere resiliencia.
    /// </para>
    /// </remarks>
    /// <example>
    /// Notificar a todos los clientes sobre un nuevo producto:
    /// <code>
    /// var notification = new WebSocketMessage
    /// {
    ///     Type = "FUNKO_CREATED",
    ///     Payload = JsonSerializer.Serialize(funkoDto),
    ///     Timestamp = DateTime.UtcNow
    /// };
    /// 
    /// await connectionManager.BroadcastAsync(JsonSerializer.Serialize(notification));
    /// </code>
    /// </example>
    public async Task BroadcastAsync(string message)
    {
        var connections = _connections
            .Where(kvp => kvp.Value.State == WebSocketState.Open)
            .ToList();
        
        logger.LogInformation("Broadcast a {Count} conexiones", connections.Count);

        foreach (var (connectionId, webSocket) in connections)
        {
            await SendMessageAsync(connectionId, message);
        }
    }

    /// <summary>
    /// Envía un mensaje a todas las conexiones de un grupo específico.
    /// </summary>
    /// <param name="groupName">El nombre del grupo/canal al que enviar el mensaje.</param>
    /// <param name="message">El mensaje de texto a enviar.</param>
    /// <returns>Una tarea que representa la operación asíncrona de envío grupal.</returns>
    /// <remarks>
    /// <para>
    /// Este método envía mensajes en paralelo a todas las conexiones del grupo usando <see cref="Task.WhenAll"/>.
    /// Solo se envía a conexiones que estén en estado Open.
    /// </para>
    /// <para>
    /// Si el grupo no existe o está vacío, el método no realiza ninguna acción.
    /// </para>
    /// <para>
    /// <strong>Casos de uso:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Notificar solo a usuarios suscritos a una categoría específica</description></item>
    /// <item><description>Enviar actualizaciones a usuarios en una sala de chat</description></item>
    /// <item><description>Notificaciones por región geográfica</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Agregar conexiones a un grupo
    /// connectionManager.AddToGroup(connectionId1, "categoria:marvel");
    /// connectionManager.AddToGroup(connectionId2, "categoria:marvel");
    /// 
    /// // Enviar mensaje solo a ese grupo
    /// var message = JsonSerializer.Serialize(new WebSocketMessage
    /// {
    ///     Type = "CATEGORY_UPDATE",
    ///     Payload = "Nuevos Funkos de Marvel disponibles"
    /// });
    /// 
    /// await connectionManager.SendToGroupAsync("categoria:marvel", message);
    /// </code>
    /// </example>
    public async Task SendToGroupAsync(string groupName, string message)
    {
        if (_userConnections.TryGetValue(groupName, out var connections))
        {
            var tasks = connections
                .Where(id => _connections.TryGetValue(id, out var ws) && ws.State == WebSocketState.Open)
                .Select(id => SendMessageAsync(id, message));

            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Agrega una conexión a un grupo/canal específico.
    /// </summary>
    /// <param name="connectionId">El identificador de la conexión a agregar.</param>
    /// <param name="groupName">El nombre del grupo al que agregar la conexión.</param>
    /// <remarks>
    /// <para>
    /// Si el grupo no existe, se crea automáticamente.
    /// Una conexión puede pertenecer a múltiples grupos simultáneamente.
    /// </para>
    /// <para>
    /// Este método es thread-safe y puede ser llamado concurrentemente.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Agrupar por categorías de interés
    /// connectionManager.AddToGroup(connectionId, "categoria:marvel");
    /// connectionManager.AddToGroup(connectionId, "categoria:dc");
    /// 
    /// // Agrupar por tipo de notificación
    /// connectionManager.AddToGroup(connectionId, "notificaciones:precios");
    /// </code>
    /// </example>
    public void AddToGroup(string connectionId, string groupName)
    {
        _userConnections.GetOrAdd(groupName, _ => new HashSet<string>()).Add(connectionId);
    }

    /// <summary>
    /// Elimina una conexión de un grupo específico.
    /// </summary>
    /// <param name="connectionId">El identificador de la conexión a eliminar.</param>
    /// <param name="groupName">El nombre del grupo del que eliminar la conexión.</param>
    /// <remarks>
    /// Si el grupo no existe o la conexión no pertenece al grupo, el método no realiza ninguna acción.
    /// Este método es thread-safe.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Usuario se desuscribe de notificaciones de una categoría
    /// connectionManager.RemoveFromGroup(connectionId, "categoria:marvel");
    /// </code>
    /// </example>
    public void RemoveFromGroup(string connectionId, string groupName)
    {
        if (_userConnections.TryGetValue(groupName, out var connections))
        {
            connections.Remove(connectionId);
        }
    }

    /// <summary>
    /// Obtiene todas las conexiones WebSocket activas (en estado Open).
    /// </summary>
    /// <returns>
    /// Una colección enumerable de instancias de <see cref="WebSocket"/> que están en estado Open.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Este método filtra automáticamente las conexiones cerradas o en otros estados.
    /// El resultado es una instantánea en el momento de la consulta; las conexiones pueden cambiar de estado después.
    /// </para>
    /// <para>
    /// <strong>Uso típico:</strong> Monitoreo, estadísticas o inspección de conexiones activas.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activeConnections = connectionManager.GetAllConnections();
    /// var count = activeConnections.Count();
    /// logger.LogInformation("Conexiones activas: {Count}", count);
    /// </code>
    /// </example>
    public IEnumerable<WebSocket> GetAllConnections()
    {
        return _connections.Values.Where(c => c.State == WebSocketState.Open);
    }
}

/// <summary>
/// Representa la estructura estándar de un mensaje WebSocket en la aplicación.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase define el formato común para todos los mensajes enviados a través de WebSocket,
/// proporcionando una estructura consistente para que los clientes puedan parsear y procesar mensajes.
/// </para>
/// <para>
/// El mensaje se serializa típicamente a JSON antes de enviarse a través del WebSocket.
/// </para>
/// </remarks>
/// <example>
/// Crear y enviar un mensaje de creación de Funko:
/// <code>
/// var message = new WebSocketMessage
/// {
///     Type = "FUNKO_CREATED",
///     Payload = JsonSerializer.Serialize(new 
///     { 
///         id = 123, 
///         nombre = "Batman Dark Knight",
///         precio = 29.99 
///     }),
///     Timestamp = DateTime.UtcNow
/// };
/// 
/// var json = JsonSerializer.Serialize(message);
/// await connectionManager.BroadcastAsync(json);
/// </code>
/// 
/// Ejemplo de mensaje recibido por el cliente (JSON):
/// <code>
/// {
///   "type": "FUNKO_CREATED",
///   "payload": "{\"id\":123,\"nombre\":\"Batman\",\"precio\":29.99}",
///   "timestamp": "2024-01-15T10:30:00Z"
/// }
/// </code>
/// 
/// Tipos de mensaje comunes:
/// <list type="bullet">
/// <item><description>FUNKO_CREATED - Se creó un nuevo producto</description></item>
/// <item><description>FUNKO_UPDATED - Se actualizó un producto existente</description></item>
/// <item><description>FUNKO_DELETED - Se eliminó un producto</description></item>
/// <item><description>CATEGORY_UPDATED - Se actualizó una categoría</description></item>
/// <item><description>ERROR - Ocurrió un error</description></item>
/// <item><description>PING - Mensaje de keep-alive</description></item>
/// </list>
/// </example>
public class WebSocketMessage
{
    /// <summary>
    /// Obtiene o establece el tipo de mensaje.
    /// </summary>
    /// <value>
    /// Una cadena que identifica el tipo de evento o acción del mensaje.
    /// Por convención, se usa UPPER_SNAKE_CASE.
    /// </value>
    /// <remarks>
    /// El cliente usa este campo para determinar cómo procesar el mensaje (routing de eventos).
    /// </remarks>
    /// <example>
    /// "FUNKO_CREATED", "FUNKO_UPDATED", "FUNKO_DELETED", "ERROR", "PING"
    /// </example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la carga útil del mensaje.
    /// </summary>
    /// <value>
    /// Una cadena opcional que contiene los datos del mensaje, típicamente JSON serializado.
    /// Puede ser <see langword="null"/> para mensajes sin datos (como PING).
    /// </value>
    /// <remarks>
    /// <para>
    /// Para mantener flexibilidad, se usa string en lugar de un objeto tipado.
    /// Los clientes deben deserializar el payload según el tipo de mensaje.
    /// </para>
    /// <para>
    /// <strong>Alternativa:</strong> Considere usar <c>object?</c> o <c>JsonElement</c>
    /// si desea serialización automática de objetos complejos.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Payload con datos de un Funko
    /// Payload = JsonSerializer.Serialize(new FunkoResponseDto(...))
    /// 
    /// // Payload con mensaje simple
    /// Payload = "Operación completada exitosamente"
    /// 
    /// // Sin payload
    /// Payload = null
    /// </code>
    /// </example>
    public string? Payload { get; set; }

    /// <summary>
    /// Obtiene o establece la marca de tiempo UTC del mensaje.
    /// </summary>
    /// <value>
    /// Un <see cref="DateTime"/> en UTC que indica cuándo se creó el mensaje.
    /// Por defecto se establece en el momento de creación del objeto.
    /// </value>
    /// <remarks>
    /// <para>
    /// Útil para:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Ordenamiento temporal de eventos en el cliente</description></item>
    /// <item><description>Detección de mensajes obsoletos o duplicados</description></item>
    /// <item><description>Auditoría y logging</description></item>
    /// <item><description>Cálculo de latencia de red</description></item>
    /// </list>
    /// <para>
    /// Se usa UTC para evitar problemas con zonas horarias.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var message = new WebSocketMessage
    /// {
    ///     Type = "FUNKO_CREATED",
    ///     Payload = "...",
    ///     Timestamp = DateTime.UtcNow
    /// };
    /// 
    /// // El cliente puede calcular latencia
    /// var latency = DateTime.UtcNow - message.Timestamp;
    /// </code>
    /// </example>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}