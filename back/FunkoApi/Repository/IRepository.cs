
namespace FunkoApi.Repository;

/// <summary>
/// Interfaz genérica para repositorios que define operaciones básicas CRUD de lectura y creación.
/// </summary>
/// <typeparam name="T">Tipo de entidad que maneja el repositorio.</typeparam>
/// <typeparam name="ID">Tipo del identificador único de la entidad.</typeparam>
public interface IRepository<T,ID>
{
    /// <summary>
    /// Obtiene todas las entidades del repositorio de forma asíncrona.
    /// </summary>
    /// <returns>Una lista conteniendo todas las entidades.</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Busca una entidad por su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="id">El identificador único de la entidad a buscar.</param>
    /// <returns>La entidad encontrada o null si no existe.</returns>
    Task<T?> GetByIdAsync(ID id);

    /// <summary>
    /// Agrega una nueva entidad al repositorio de forma asíncrona.
    /// </summary>
    /// <param name="entity">La entidad a agregar.</param>
    /// <returns>La entidad agregada con sus datos actualizados (ej. ID generado).</returns>
    Task<T> AddAsync(T entity);
}