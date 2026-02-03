using FunkoApi.Service.Cache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extensiones de configuración de caché.
/// </summary>
public static class CacheConfig
{
    /// <summary>
    /// Configura el servicio de caché.
    /// Desarrollo: MemoryCache.
    /// Producción: Redis.
    /// </summary>
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        
            Log.Information("💾 Configurando caché Redis (producción)...");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "redis:6379,password=redispass123";
                options.InstanceName = "FunkoApi:";
            });
            services.TryAddScoped<ICacheService, CacheService>();
        return services;
    }
}