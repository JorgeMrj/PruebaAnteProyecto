using CSharpFunctionalExtensions;
using FunkoApi.Dto.Categories;
using FunkoApi.Error;
using FunkoApi.Handler.Categorias;
using FunkoApi.mapper;
using FunkoApi.Models;
using FunkoApi.Repository.Category;
using FunkoApi.Service.Cache;

namespace FunkoApi.Service.Category;

/// <summary>
/// Implementación del servicio de categorías.
/// Maneja la lógica de negocio, cacheo y notificaciones WebSocket.
/// </summary>
public class CategoriaService(ILogger<CategoriaService> logger,ICategoryRepository repository,ICacheService cache, CategoriaWebSocketHandler webSocketHandler) : ICategoriaService
{
    private const string CacheKey = "Categoria_";

    /// <inheritdoc />
    public async Task<List<CategoriaResponseDto>> GetCategoriasAsync()
    {
        logger.LogInformation("Getting categorias");
        return await Task.FromResult(repository.GetAllAsync().Result.Select(it => it.ToDto()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<CategoriaResponseDto, CategoriaError>> GetCategoriaAsync(string id)
    {
        return await cache.GetAsync<Categoria>(CacheKey+id) is { } cached
            ? Result.Success<CategoriaResponseDto, CategoriaError>(cached.ToDto())
            : await repository.GetByIdAsync(id) is { } categoria
            ? Result.Success<CategoriaResponseDto, CategoriaError>(categoria.ToDto())
                .Tap(_ =>
                {
                    logger.LogInformation("getting categoria {id}", id);
                    cache.SetAsync(CacheKey+id, categoria);
                })
            : Result.Failure<CategoriaResponseDto, CategoriaError>(
                new CategoriaNotFoundError(($"no se ha encontrado categoria con nombre: {id}", id).ToString()))
                .TapError(_ => logger.LogWarning("categoria not found with name: {id}", id));
    }

    /// <inheritdoc />
    public async Task<Result<CategoriaResponseDto, CategoriaError>> SaveCategoriaAsync(CategoriaRequestDto request)
    {
        return await repository.AddAsync(request.ToModel()) is { } categoria
            ? Result.Success<CategoriaResponseDto, CategoriaError>(categoria.ToDto())
                .Tap(_ =>
                {
                    logger.LogInformation("saving categoria {id}", categoria.Id);
                    NotificarWebSocketCategoria(categoria.ToDto(),CategoriaNotificationType.CREATED);
                })
            : Result.Failure<CategoriaResponseDto, CategoriaError>(
                    new CategoriaBadRequestError("No se pudo guardar la categoria"))
                .TapError(_ => logger.LogWarning("No se pudo guardar la categoria"));
    }

    /// <inheritdoc />
    public async Task<Result<CategoriaResponseDto, CategoriaError>> DeleteCategoriaAsync(Guid id)
    {
        return await repository.DeleteAsync(id) is { } categoria
            ? Result.Success<CategoriaResponseDto, CategoriaError>(categoria.ToDto())
                .Tap(_ =>
                {
                    logger.LogInformation("deleting categoria {id}", id);
                    cache.RemoveAsync(CacheKey+categoria.Nombre);
                    NotificarWebSocketCategoria(categoria.ToDto(), CategoriaNotificationType.DELETED);
                })
            : Result.Failure<CategoriaResponseDto, CategoriaError>(
                    new CategoriaNotFoundError(($"no se encontro la categoria con id: {id}", id).ToString()))
                .TapError(_ => logger.LogWarning("categoria no encontrada con id: {id}", id));
    }

    /// <inheritdoc />
    public async Task<Result<CategoriaResponseDto, CategoriaError>> UpdateCategoriaAsync(Guid id, CategoriaRequestDto request)
    {
        return await repository.UpdateAsync(id, request.ToModel()) is { } categoria
            ? Result.Success<CategoriaResponseDto, CategoriaError>(categoria.ToDto())
                .Tap(_ =>
                {
                    cache.RemoveAsync(CacheKey+categoria.Nombre);
                    logger.LogInformation("updating categoria {id}", id);
                    NotificarWebSocketCategoria(categoria.ToDto(), CategoriaNotificationType.UPDATED);
                    
                })
            : Result.Failure<CategoriaResponseDto, CategoriaError>(
                    new CategoriaNotFoundError(($"categoria no encontrada con id: {id}", id).ToString()))
                .TapError(_ => logger.LogWarning("categoria con id: {id}", id));
    }

    private void NotificarWebSocketCategoria(CategoriaResponseDto categoria, string type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        _ = Task.Run(async () =>
        {
            try
            {
                await webSocketHandler.NotifyAsync(new CategoriaNotificacion(
                    type,
                    categoria.Id,
                    categoria
                ));
                logger.LogDebug("Notificación WebSocket enviada tras crear Categoria: {CategoriaId}", categoria.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error en notificación WebSocket al crear Categoria: {CategoriaId}", categoria.Id);
            }
        });
    }
}