using CSharpFunctionalExtensions;
using FunkoApi.Dto.Categories;
using FunkoApi.Error;
using FunkoApi.Service.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FunkoApi.Models.User;

namespace FunkoApi.Controllers;

/// <summary>
/// Proporciona endpoints API para la gestión de categorías de Funkos.
/// </summary>
/// <remarks>
/// Este controlador maneja todas las operaciones CRUD (Crear, Leer, Actualizar, Eliminar)
/// para las categorías de productos Funko.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriaController(ICategoriaService service) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las categorías disponibles.
    /// </summary>
    /// <returns>
    /// Una colección de objetos <see cref="CategoriaResponseDto"/> que representan todas las categorías.
    /// </returns>
    /// <response code="200">Devuelve la lista de categorías.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// GET /api/categoria
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync()
    {
        return Ok(await service.GetCategoriasAsync());
    }

    /// <summary>
    /// Obtiene una categoría específica por su identificador único.
    /// </summary>
    /// <param name="id">El identificador único (string) de la categoría a obtener.</param>
    /// <returns>
    /// Un objeto <see cref="CategoriaResponseDto"/> que representa la categoría solicitada.
    /// </returns>
    /// <response code="200">Devuelve la categoría encontrada.</response>
    /// <response code="400">Si el formato del ID es inválido.</response>
    /// <response code="404">Si no se encuentra la categoría con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// GET /api/categoria/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(string id)
    {
        return await service.GetCategoriaAsync(id).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                CategoriaNotFoundError => NotFound(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    /// <param name="id">El identificador único (GUID) de la categoría a actualizar.</param>
    /// <param name="categoria">El objeto <see cref="CategoriaRequestDto"/> con los datos actualizados de la categoría.</param>
    /// <returns>
    /// Un objeto <see cref="CategoriaResponseDto"/> que representa la categoría actualizada.
    /// </returns>
    /// <response code="200">La categoría se actualizó correctamente.</response>
    /// <response code="400">Si los datos de entrada son inválidos.</response>
    /// <response code="404">Si no se encuentra la categoría con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// PUT /api/categoria/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// {
    ///   "nombre": "Categoría Actualizada",
    ///   "descripcion": "Nueva descripción"
    /// }
    /// </example>
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.ADMIN)]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CategoriaRequestDto categoria)
    {
        return await service.UpdateCategoriaAsync(id, categoria).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                CategoriaNotFoundError => NotFound(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    /// <param name="categoria">El objeto <see cref="CategoriaRequestDto"/> con los datos de la nueva categoría.</param>
    /// <returns>
    /// Un objeto <see cref="CategoriaResponseDto"/> que representa la categoría creada.
    /// </returns>
    /// <response code="201">La categoría se creó correctamente. Devuelve la ubicación del recurso en el header Location.</response>
    /// <response code="400">Si los datos de entrada son inválidos o no cumplen las reglas de validación.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// POST /api/categoria
    /// {
    ///   "nombre": "Nueva Categoría",
    ///   "descripcion": "Descripción de la categoría"
    /// }
    /// </example>
    [HttpPost]
    [Authorize(Roles = UserRoles.ADMIN)]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostAsync([FromBody] CategoriaRequestDto categoria)
    {
        return await service.SaveCategoriaAsync(categoria).Match(
            onSuccess: response => Created($"/api/categoria/{response.Id}", response),
            onFailure: error => error switch
            {
                CategoriaBadRequestError => BadRequest(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Elimina una categoría existente.
    /// </summary>
    /// <param name="id">El identificador único (GUID) de la categoría a eliminar.</param>
    /// <returns>
    /// Un objeto <see cref="CategoriaResponseDto"/> que representa la categoría eliminada.
    /// </returns>
    /// <response code="200">La categoría se eliminó correctamente.</response>
    /// <response code="404">Si no se encuentra la categoría con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <remarks>
    /// ADVERTENCIA: Esta operación es irreversible. Asegúrese de que la categoría 
    /// no esté siendo utilizada por otros recursos antes de eliminarla.
    /// </remarks>
    /// <example>
    /// DELETE /api/categoria/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.ADMIN)]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        return await service.DeleteCategoriaAsync(id).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                CategoriaNotFoundError => NotFound(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }
}