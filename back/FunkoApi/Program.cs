using System.Text;
using FunkoApi.config;
using FunkoApi.Infrastructures;
using FunkoApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

/// <summary>
/// Punto de entrada de la aplicación API REST y GraphQL.
/// Configura servicios, pipeline de middlewares y arranque.
/// </summary>

Log.Logger= SerilogConfig.Configure().CreateLogger();
Console.OutputEncoding = Encoding.UTF8; 
var builder = WebApplication.CreateBuilder(args);
//configuracion log
builder.Host.UseSerilog();
var services = builder.Services;
// negociacion de serializables
services.AddMvcControllers();

//base de datos en possgress
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FunkoDbContext>(options =>
    options.UseNpgsql(connectionString));
// Auth
services.AddAuthentication(builder.Configuration);
// repositorios
services.AddRepositories();
// servicios
services.AddServices();
// cache
services.AddCache();
services.AddEmail(builder.Environment);

services.AddStorage();
services.AddWebSockets();
services.AddGraphQl(builder.Environment);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();


app.UseGraphiQL();
app.UseGlobalExceptionHandler();
app.UseCorsPolicy();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapWebSocketEndpoints();
app.UseStaticFiles();
app.MapControllers();
app.InitializeDatabaseAsync();
app.InitializeStorage();
app.MapGraphQL();

try
{
    Log.Information("🚀 Iniciando aplicación FunkoApi...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}