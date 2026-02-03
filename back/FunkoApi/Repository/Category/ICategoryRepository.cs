using FunkoApi.Models;

namespace FunkoApi.Repository.Category;

/// <summary>
/// Interfaz específica para el repositorio de categorías.
/// Extiende de IRepository para operaciones básicas y agrega operaciones específicas.
/// </summary>
public interface ICategoryRepository : IRepository<Categoria,string>
{
    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    /// <param name="id">El GUID de la categoría a actualizar.</param>
    /// <param name="categoria">El objeto con los nuevos datos.</param>
    /// <returns>La categoría actualizada o null si no se encontró.</returns>
    Task<Categoria?> UpdateAsync(Guid id, Categoria categoria);

    /// <summary>
    /// Elimina una categoría por su ID.
    /// </summary>
    /// <param name="id">El GUID de la categoría a eliminar.</param>
    /// <returns>La categoría eliminada o null si no se encontró.</returns>
    Task<Categoria?> DeleteAsync(Guid id);

    /// <summary>
    /// Devuelve una consulta IQueryable de categorías sin seguimiento de cambios (read-only).
    /// </summary>
    /// <returns>IQueryable de categorías.</returns>
    IQueryable<Categoria> FindAllAsNoTracking();
}