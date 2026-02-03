using CSharpFunctionalExtensions;
using FunkoApi.Dto.Categories;
using FunkoApi.Error;

namespace FunkoApi.Service.Category;

/// <summary>
/// Interfaz que define los servicios lógicos para la gestión de categorías.
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Obtiene todas las categorías disponibles.
    /// </summary>
    /// <returns>Una lista de DTOs de todas las categorías.</returns>
    Task<List<CategoriaResponseDto>> GetCategoriasAsync();

    /// <summary>
    /// Obtiene una categoría por su nombre o identificador.
    /// </summary>
    /// <param name="id">El nombre o identificador de la categoría.</param>
    /// <returns>Un Result con el DTO de la categoría o un error si no se encuentra.</returns>
    Task<Result<CategoriaResponseDto,CategoriaError>> GetCategoriaAsync(string id);

    /// <summary>
    /// Crea y guarda una nueva categoría.
    /// </summary>
    /// <param name="request">DTO con los datos de creación.</param>
    /// <returns>Un Result con el DTO de la categoría creada o un error si falla.</returns>
    Task<Result<CategoriaResponseDto,CategoriaError>> SaveCategoriaAsync( CategoriaRequestDto request);

    /// <summary>
    /// Elimina una categoría por su ID.
    /// </summary>
    /// <param name="id">GUID de la categoría a eliminar.</param>
    /// <returns>Un Result con el DTO de la categoría eliminada o un error si no existe.</returns>
    Task<Result<CategoriaResponseDto,CategoriaError>> DeleteCategoriaAsync(Guid id);

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    /// <param name="id">GUID de la categoría a actualizar.</param>
    /// <param name="request">DTO con los datos actualizados.</param>
    /// <returns>Un Result con el DTO de la categoría actualizada o un error si falla.</returns>
    Task<Result<CategoriaResponseDto,CategoriaError>> UpdateCategoriaAsync(Guid id,CategoriaRequestDto request);
}