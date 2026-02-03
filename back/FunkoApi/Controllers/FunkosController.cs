using CSharpFunctionalExtensions;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;
using FunkoApi.Service.Funkos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FunkoApi.Models.User;

namespace FunkoApi.Controllers;

/// <summary>
/// Proporciona endpoints API para la gestión de productos Funko Pop.
/// </summary>
/// <remarks>
/// Este controlador maneja todas las operaciones CRUD (Crear, Leer, Actualizar, Eliminar)
/// para los productos Funko, incluyendo la gestión de imágenes asociadas.
/// Soporta operaciones con archivos mediante multipart/form-data para las imágenes de los productos.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FunkosController(IFunkoService funkoService) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los productos Funko disponibles.
    /// </summary>
    /// <returns>
    /// Una colección de objetos <see cref="FunkoResponseDto"/> que representan todos los productos Funko.
    /// </returns>
    /// <response code="200">Devuelve la lista completa de productos Funko.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// GET /api/funkos
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FunkoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync()
    {
        return Ok(await funkoService.GetFunkosAsync());
    }

    /// <summary>
    /// Obtiene un producto Funko específico por su identificador único.
    /// </summary>
    /// <param name="id">El identificador numérico único del producto Funko.</param>
    /// <returns>
    /// Un objeto <see cref="FunkoResponseDto"/> que representa el producto Funko solicitado.
    /// </returns>
    /// <response code="200">Devuelve el producto Funko encontrado.</response>
    /// <response code="404">Si no se encuentra el producto Funko con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <example>
    /// GET /api/funkos/123
    /// </example>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(long id)
    {
        return await funkoService.GetFunkoAsync(id).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                FunkoNotFoundError => NotFound(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Crea un nuevo producto Funko con imagen opcional.
    /// </summary>
    /// <param name="nombre">El nombre descriptivo del producto Funko.</param>
    /// <param name="price">El precio del producto en la moneda base del sistema.</param>
    /// <param name="categoria">La categoría a la que pertenece el producto Funko.</param>
    /// <param name="file">Archivo de imagen opcional del producto. Formatos aceptados: JPG, PNG, GIF.</param>
    /// <returns>
    /// Un objeto <see cref="FunkoResponseDto"/> que representa el producto Funko creado.
    /// </returns>
    /// <response code="201">El producto Funko se creó correctamente. Devuelve la ubicación del recurso en el header Location.</response>
    /// <response code="400">Si los datos de entrada son inválidos, no cumplen las reglas de validación, o hay un error al procesar la imagen.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <remarks>
    /// Este endpoint acepta datos en formato multipart/form-data para soportar la carga de archivos.
    /// El archivo de imagen es opcional, pero si se proporciona, debe cumplir con las restricciones de tamaño y formato.
    /// </remarks>
    /// <example>
    /// POST /api/funkos
    /// Content-Type: multipart/form-data
    /// 
    /// nombre=Funko Pop Batman
    /// price=29.99
    /// categoria=DC Comics
    /// file=[binary_image_data]
    /// </example>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN)]
    public async Task<IActionResult> PostAsync(
        [FromForm] string nombre,
        [FromForm] double price,
        [FromForm] string categoria,
        [FromForm] IFormFile? file)
    {
        var request = new FunkoRequestDto
        {
            Nombre = nombre,
            Price = price,
            Categoria = categoria
        };
        
        return await funkoService.SaveFunkoAsync(request, file).Match(
            onSuccess: response => Created($"/api/funkos/{response.Id}", response),
            onFailure: error => error switch
            {
                FunkoValidationError => BadRequest(new { message = error.Error }),
                FunkoStorageError => BadRequest(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Actualiza un producto Funko existente, incluyendo opcionalmente su imagen.
    /// </summary>
    /// <param name="id">El identificador numérico único del producto Funko a actualizar.</param>
    /// <param name="nombre">El nombre actualizado del producto Funko.</param>
    /// <param name="price">El precio actualizado del producto.</param>
    /// <param name="categoria">La categoría actualizada del producto Funko.</param>
    /// <param name="file">Archivo de imagen opcional para reemplazar la imagen actual. Formatos aceptados: JPG, PNG, GIF.</param>
    /// <returns>
    /// Un objeto <see cref="FunkoResponseDto"/> que representa el producto Funko actualizado.
    /// </returns>
    /// <response code="200">El producto Funko se actualizó correctamente.</response>
    /// <response code="400">Si los datos de entrada son inválidos o hay un error al procesar la imagen.</response>
    /// <response code="404">Si no se encuentra el producto Funko con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <remarks>
    /// Este endpoint acepta datos en formato multipart/form-data para soportar la actualización de archivos.
    /// Si se proporciona un nuevo archivo de imagen, reemplazará la imagen anterior del producto.
    /// Si no se proporciona archivo, se mantendrá la imagen existente.
    /// </remarks>
    /// <example>
    /// PUT /api/funkos/123
    /// Content-Type: multipart/form-data
    /// 
    /// nombre=Funko Pop Batman Actualizado
    /// price=34.99
    /// categoria=DC Comics
    /// file=[binary_image_data]
    /// </example>
    [HttpPut("{id:long}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN)]
    public async Task<IActionResult> PutAsync(
        long id,
        [FromForm] string nombre,
        [FromForm] double price,
        [FromForm] string categoria,
        [FromForm] IFormFile? file)
    {
        var request = new FunkoRequestDto
        {
            Nombre = nombre,
            Price = price,
            Categoria = categoria
        };
        
        return await funkoService.UpdateFunkoAsync(id, request, file).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                FunkoValidationError => BadRequest(new { message = error.Error }),
                FunkoNotFoundError => NotFound(new { message = error.Error }),
                FunkoStorageError => BadRequest(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }

    /// <summary>
    /// Elimina un producto Funko existente del sistema.
    /// </summary>
    /// <param name="id">El identificador numérico único del producto Funko a eliminar.</param>
    /// <returns>
    /// Un objeto <see cref="FunkoResponseDto"/> que representa el producto Funko eliminado.
    /// </returns>
    /// <response code="200">El producto Funko se eliminó correctamente.</response>
    /// <response code="404">Si no se encuentra el producto Funko con el ID especificado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    /// <remarks>
    /// ADVERTENCIA: Esta operación es irreversible y también eliminará la imagen asociada al producto si existe.
    /// Asegúrese de que el producto no tenga referencias pendientes en otros módulos del sistema antes de eliminarlo.
    /// </remarks>
    /// <example>
    /// DELETE /api/funkos/123
    /// </example>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN)]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        return await funkoService.DeleteFunkoAsync(id).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                FunkoNotFoundError => NotFound(new { message = error.Error }),
                _ => StatusCode(500, new { message = error.Error })
            });
    }
}