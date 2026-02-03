using FunkoApi.Models;

namespace FunkoApi.Repository.funkos;

/// <summary>
/// Interfaz específica para el repositorio de Funkos.
/// Extiende de IRepository para operaciones básicas y agrega operaciones específicas.
/// </summary>
public interface IFunkoRepository : IRepository<Funko,long>
{
    /// <summary>
    /// Actualiza un Funko existente.
    /// </summary>
    /// <param name="id">El ID del Funko a actualizar.</param>
    /// <param name="newFunko">El objeto con los nuevos datos.</param>
    /// <returns>El Funko actualizado o null si no se encontró.</returns>
    Task<Funko?> UpdateAsync(long id, Funko newFunko);

    /// <summary>
    /// Elimina un Funko por su ID.
    /// </summary>
    /// <param name="id">El ID del Funko a eliminar.</param>
    /// <returns>El Funko eliminado o null si no se encontró.</returns>
    Task<Funko?> DeleteAsync(long id);

    /// <summary>
    /// Devuelve una consulta IQueryable de Funkos sin seguimiento de cambios (read-only).
    /// </summary>
    /// <returns>IQueryable de Funkos.</returns>
    IQueryable<Funko> FindAllAsNoTracking();
}