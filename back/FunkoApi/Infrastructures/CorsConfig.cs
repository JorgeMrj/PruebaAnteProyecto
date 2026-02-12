using Serilog;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extensiones de configuración de CORS.
/// </summary>
public static class CorsConfig
{
    /// <summary>
    /// Configura la política CORS según el entorno.
    /// Desarrollo: AllowAll (permite todo)
    /// Producción: Solo orígenes configurados en Cors:AllowedOrigins
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Log.Information("🌐 Configurando CORS para {Environment}...", isDevelopment ? "DESARROLLO" : "PRODUCCIÓN");

        return services.AddCors(options =>
        {
            if (isDevelopment)
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                Log.Information("🌐 CORS: AllowAll (desarrollo)");
            }
            else
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                     ?? throw new InvalidOperationException("Cors:AllowedOrigins no configurado");

                options.AddPolicy("ProductionPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        // Verificar orígenes exactos
                        if (allowedOrigins.Any(ao => ao.Equals(origin, StringComparison.OrdinalIgnoreCase)))
                            return true;
                        
                        // Verificar patrones con wildcard
                        foreach (var allowedOrigin in allowedOrigins)
                        {
                            if (allowedOrigin.Contains("*"))
                            {
                                var pattern = "^" + System.Text.RegularExpressions.Regex.Escape(allowedOrigin)
                                    .Replace("\\*", ".*") + "$";
                                if (System.Text.RegularExpressions.Regex.IsMatch(origin, pattern, 
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                    return true;
                            }
                        }
                        return false;
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
                Log.Information("🌐 CORS: ProductionPolicy con {Count} orígenes", allowedOrigins.Length);
            }
        });
    }
}