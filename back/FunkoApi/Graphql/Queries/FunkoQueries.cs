using FunkoApi.Models;
using FunkoApi.Repository.Category;
using FunkoApi.Repository.funkos;

namespace FunkoApi.Graphql.Queries;

/// <summary>
/// Consultas GraphQL de la tienda.
/// </summary>
public class FunkoQuery
{
    /// <summary>Obtiene todos los funkos (proyección habilitada).</summary>
    /// <param name="funkoRepository">Repositorio de funkos.</param>
    /// <returns>IQueryable de funkos.</returns>
    [UseProjection]
    public IQueryable<Funko> GetFunkos([Service] IFunkoRepository funkoRepository) =>
        funkoRepository.FindAllAsNoTracking();

    /// <summary>Obtiene un funko por ID.</summary>
    /// <param name="id">ID del funko.</param>
    /// <param name="funkoRepository">Repositorio de funkos.</param>
    /// <returns>funko encontrado o null.</returns>
    [UseFirstOrDefault]
    public async Task<Funko?> GetFunko(long id, [Service] IFunkoRepository funkoRepository) =>
        await funkoRepository.GetByIdAsync(id);

    /// <summary>Obtiene funkos paginados.</summary>
    /// <param name="funkoRepository">Repositorio de funkos.</param>
    /// <returns>IQueryable de funkos paginados.</returns>
    [UsePaging(MaxPageSize = 100, DefaultPageSize = 10)]
    public IQueryable<Funko> GetFunkosPaged([Service] IFunkoRepository funkoRepository) =>
        funkoRepository.FindAllAsNoTracking();

    /// <summary>Obtiene todas las categorías.</summary>
    /// <param name="categoriaRepository">Repositorio de categorías.</param>
    /// <returns>IQueryable de categorías.</returns>
    [UseProjection]
    public IQueryable<Categoria> GetCategorias([Service] ICategoryRepository categoriaRepository) =>
        categoriaRepository.FindAllAsNoTracking();

    /// <summary>Obtiene una categoría por ID.</summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="categoriaRepository">Repositorio de categorías.</param>
    /// <returns>Categoría encontrada o null.</returns>
    [UseFirstOrDefault]
    public async Task<Categoria?> GetCategoria(string id, [Service] ICategoryRepository categoriaRepository) =>
        await categoriaRepository.GetByIdAsync(id);

    /// <summary>Obtiene categorías paginadas.</summary>
    /// <param name="categoriaRepository">Repositorio de categorías.</param>
    /// <returns>IQueryable de categorías paginadas.</returns>
    [UsePaging(MaxPageSize = 100, DefaultPageSize = 10)]
    public IQueryable<Categoria> GetCategoriasPaged([Service] ICategoryRepository categoriaRepository) =>
        categoriaRepository.FindAllAsNoTracking();
}