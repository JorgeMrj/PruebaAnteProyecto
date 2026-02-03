namespace FunkoApi.Error;

/// <summary>
/// Clase base abstracta para todos los errores relacionados con operaciones de categorías.
/// </summary>
/// <remarks>
/// <para>
/// Esta jerarquía de errores permite el uso de pattern matching fuertemente tipado
/// para mapear errores de la capa de servicio a respuestas HTTP apropiadas en los controladores.
/// </para>
/// <para>
/// Utiliza records inmutables para garantizar que los mensajes de error no puedan ser modificados
/// después de su creación, proporcionando thread-safety y predictibilidad.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de uso con CSharpFunctionalExtensions:
/// <code>
/// // En el servicio
/// public async Task&lt;Result&lt;CategoriaResponseDto, CategoriaError&gt;&gt; GetCategoriaAsync(Guid id)
/// {
///     var categoria = await _repository.FindByIdAsync(id);
///     return categoria == null
///         ? new CategoriaNotFoundError($"Categoría {id} no encontrada")
///         : Result.Success&lt;CategoriaResponseDto, CategoriaError&gt;(MapToDto(categoria));
/// }
/// 
/// // En el controlador
/// return await _service.GetCategoriaAsync(id).Match(
///     onSuccess: dto => Ok(dto),
///     onFailure: error => error switch
///     {
///         CategoriaNotFoundError => NotFound(new { message = error.Error }),
///         CategoriaValidationError => BadRequest(new { message = error.Error }),
///         CategoriaBadRequestError => BadRequest(new { message = error.Error }),
///         CategoriaStorageError => StatusCode(500, new { message = error.Error }),
///         _ => StatusCode(500, new { message = "Error inesperado" })
///     });
/// </code>
/// </example>
public abstract record CategoriaError(
    /// <summary>
    /// Mensaje descriptivo del error ocurrido.
    /// </summary>
    /// <value>
    /// Una cadena de texto inmutable que describe el error específico.
    /// </value>
    string Error);

/// <summary>
/// Error que indica que una categoría solicitada no existe en el sistema.
/// </summary>
/// <remarks>
/// Corresponde a la respuesta HTTP 404 Not Found.
/// Se utiliza en operaciones GET, PUT y DELETE cuando el recurso no se encuentra.
/// </remarks>
/// <example>
/// <code>
/// new CategoriaNotFoundError("Categoría con ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 no encontrada")
/// </code>
/// </example>
public record CategoriaNotFoundError(string Error) : CategoriaError(Error);

/// <summary>
/// Error que indica una solicitud incorrecta por reglas de negocio.
/// </summary>
/// <remarks>
/// Corresponde a la respuesta HTTP 400 Bad Request.
/// Se utiliza para violaciones de reglas de negocio como nombres duplicados.
/// </remarks>
/// <example>
/// <code>
/// new CategoriaBadRequestError("Ya existe una categoría con el nombre 'Marvel Comics'")
/// </code>
/// </example>
public record CategoriaBadRequestError(string Error) : CategoriaError(Error);

/// <summary>
/// Error que indica fallo en la validación de datos de entrada.
/// </summary>
/// <remarks>
/// Corresponde a la respuesta HTTP 400 Bad Request.
/// Se utiliza para errores de validación de formato, longitud, o restricciones de datos.
/// </remarks>
/// <example>
/// <code>
/// new CategoriaValidationError("El nombre de la categoría debe tener entre 2 y 100 caracteres")
/// </code>
/// </example>
public record CategoriaValidationError(string Error) : CategoriaError(Error);

/// <summary>
/// Error que indica un problema en la capa de persistencia o almacenamiento.
/// </summary>
/// <remarks>
/// Corresponde a la respuesta HTTP 500 Internal Server Error.
/// Se utiliza para errores de base de datos, conexión, o problemas de infraestructura.
/// </remarks>
/// <example>
/// <code>
/// new CategoriaStorageError("Error al guardar la categoría: violación de restricción de clave única")
/// </code>
/// </example>
public record CategoriaStorageError(string Error) : CategoriaError(Error);