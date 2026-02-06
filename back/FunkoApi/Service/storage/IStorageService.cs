namespace FunkoApi.Service.storage;

/// <summary>
/// Interfaz para el servicio de almacenamiento de archivos
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Inicializa el almacenamiento (crea directorios necesarios)
    /// </summary>
    Task InitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Almacena un archivo y devuelve el nombre generado
    /// </summary>
    /// <param name="file">Archivo a almacenar</param>
    /// <param name="folder">Carpeta donde guardar (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Nombre del archivo guardado</returns>
    Task<string> StoreAsync(
        IFormFile file, 
        string? folder = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Almacena un archivo desde un stream
    /// </summary>
    Task<string> StoreAsync(
        Stream stream,
        string fileName,
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carga un archivo como stream
    /// </summary>
    Task<Stream> LoadAsStreamAsync(
        string fileName, 
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la ruta completa del archivo
    /// </summary>
    string GetFilePath(string fileName, string? folder = null);

    /// <summary>
    /// Obtiene la URL pública del archivo
    /// </summary>
    string GetUrl(string fileName, string? folder = null);

    /// <summary>
    /// Verifica si un archivo existe
    /// </summary>
    bool Exists(string fileName, string? folder = null);

    /// <summary>
    /// Elimina un archivo
    /// </summary>
    Task<bool> DeleteAsync(
        string fileName, 
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina todos los archivos de una carpeta
    /// </summary>
    Task DeleteAllAsync(
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos los archivos de una carpeta
    /// </summary>
    Task<IEnumerable<string>> ListFilesAsync(string? folder = null);
}