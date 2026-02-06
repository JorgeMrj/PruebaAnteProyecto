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
                options.Configuration = "redis://red-d630v77gi27c7382gq10:6379";
                options.InstanceName = "FunkoApi:";
            });
            services.TryAddScoped<ICacheService, CacheService>();
        return services;
    }
}