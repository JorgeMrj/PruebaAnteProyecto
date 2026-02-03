using Serilog;
using Path = System.IO.Path;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extension methods para inicialización del directorio de almacenamiento.
/// </summary>
public static class StorageInitializationExtensions
{
    /// <summary>
    /// Inicializa el directorio de almacenamiento de archivos.
    /// Desarrollo: Borra y recrea el directorio.
    /// Producción: Solo crea si no existe.
    /// </summary>
    public static void InitializeStorage(this WebApplication app)
    {
        var storagePath = Path.Combine(app.Environment.WebRootPath, "uploads");
        var storageDirectory = new DirectoryInfo(storagePath);
        
            Log.Information("🖼️ [PRODUCCIÓN] Verificando directorio de almacenamiento: {Path}", storagePath);
            try
            {
                if (!storageDirectory.Exists)
                {
                    storageDirectory.Create();
                    Log.Information("✅ Directorio de almacenamiento creado");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error al verificar directorio de almacenamiento");
            }
        
    }
}