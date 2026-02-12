using Serilog;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extension methods para CORS.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Aplica la política CORS configurada según el entorno.
    /// </summary>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        // Usar siempre AllowAll ya que es lo que se registra
        Log.Information("🌐 Aplicando política CORS: AllowAll");
        return app.UseCors("AllowAll");
    }
}