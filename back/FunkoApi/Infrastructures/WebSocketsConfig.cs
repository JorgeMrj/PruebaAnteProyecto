using FunkoApi.Handler.Categorias;
using FunkoApi.Handler.Funkos;
using Serilog;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extensiones de configuración de WebSockets.
/// </summary>
public static class WebSocketsConfig
{
    /// <summary>
    /// Configura los handlers de WebSocket para notificaciones en tiempo real.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>La colección de servicios.</returns>
    /// <remarks>
    /// <para><b>Handlers registrados:</b></para>
    /// <list type="bullet">
    ///   <item><description>ProductosWebSocketHandler: Notificaciones públicas de productos.</description></item>
    ///   <item><description>PedidosWebSocketHandler: Notificaciones privadas de pedidos con JWT.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddWebSockets(this IServiceCollection services)
    {
        Log.Information("🔌 Registrando handlers de WebSocket...");
        return services
            .AddSingleton<FunkosWebSocketHandler>()
            .AddSingleton<CategoriaWebSocketHandler>();
    }
}