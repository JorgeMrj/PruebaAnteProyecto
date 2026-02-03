using FunkoApi.Service;
using FunkoApi.Service.auth;
using FunkoApi.Service.Category;
using FunkoApi.Service.Funkos;
using Serilog;
using TiendaApi.Api.Services.Auth;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extensiones de configuración de servicios de negocio.
/// </summary>
public static class ServicesConfig
{
    /// <summary>
    /// Registra todos los servicios de negocio en el contenedor de dependencias.
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        Log.Information("⚙️ Registrando servicios...");
        return services
            .AddScoped<IFunkoService, FunkoService>()
            .AddScoped<ICategoriaService, CategoriaService>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IJwtTokenExtractor, JwtTokenExtractor>()
            .AddScoped<IAuthService, AuthService>();
    }
}