namespace FunkoApi.Error;

/// <summary>
/// Clase base abstracta para todos los errores relacionados con operaciones de productos Funko.
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
/// <para>
/// <strong>Nota de diseño:</strong> Actualmente tiene redundancia innecesaria al redeclarar
/// la propiedad Error como mutable. Considere simplificar usando solo el parámetro posicional.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de uso con CSharpFunctionalExtensions:
/// <code>
/// // En el servicio
/// public async Task&lt;Result&lt;FunkoResponseDto, FunkoError&gt;&gt; GetFunkoAsync(long id)
/// {
///     var funko = await _repository.FindByIdAsync(id);
///     return funko == null
///         ? new FunkoNotFoundError($"Funko con ID {id} no encontrado")
///         : Result.Success&lt;FunkoResponseDto, FunkoError&gt;(MapToDto(funko));
/// }
/// 
/// // En el controlador
/// return await _service.GetFunkoAsync(id).Match(
///     onSuccess: dto => Ok(dto),
///     onFailure: error => error switch
///     {
///         FunkoNotFoundError => NotFound(new { message = error.Error }),
///         FunkoValidationError => BadRequest(new { message = error.Error }),
///         FunkoStorageError => StatusCode(500, new { message = error.Error }),
///         _ => StatusCode(500, new { message = "Error inesperado" })
///     });
/// </code>
/// </example>
public record FunkoError(
    /// <summary>
    /// Mensaje descriptivo del error ocurrido.
    /// </summary>
    /// <value>
    /// Una cadena de texto que describe el error específico durante la operación con productos Funko.
    /// </value>
    /// <example>
    /// "Producto Funko no encontrado", "Error al validar datos del producto"
    /// </example>
    string Error)
{
    /// <summary>
    /// Obtiene o establece el mensaje de error.
    /// </summary>
    /// <value>
    /// Cadena de texto con la descripción del error.
    /// </value>
    /// <remarks>
    /// <strong>Redundante:</strong> Esta propiedad duplica el parámetro posicional del record.
    /// Los records generan automáticamente una propiedad de solo inicialización,
    /// por lo que esta redeclaración como mutable es innecesaria y rompe la inmutabilidad.
    /// </remarks>
    public string Error { get; set; } = Error;
}

/// <summary>
/// Representa un error que ocurre cuando no se encuentra un producto Funko solicitado.
/// </summary>
/// <remarks>
/// Este error se utiliza cuando se intenta acceder, actualizar o eliminar un producto
/// que no existe en el sistema. Típicamente se mapea a una respuesta HTTP 404 Not Found.
/// </remarks>
/// <example>
/// <code>
/// return new FunkoNotFoundError($"No se encontró el producto Funko con ID {id}");
/// </code>
/// 
/// Uso en pattern matching del controlador:
/// <code>
/// return await funkoService.GetFunkoAsync(id).Match(
///     onSuccess: response => Ok(response),
///     onFailure: error => error switch
///     {
///         FunkoNotFoundError => NotFound(new { message = error.Error }),
///         _ => StatusCode(500, new { message = error.Error })
///     });
/// </code>
/// </example>
public record FunkoNotFoundError(
    /// <summary>
    /// Mensaje que describe qué producto Funko no fue encontrado.
    /// </summary>
    /// <example>
    /// "Producto Funko con ID 123 no encontrado"
    /// </example>
    string Error) : FunkoError(Error);

/// <summary>
/// Representa un error de solicitud incorrecta (Bad Request) en operaciones de productos Funko.
/// </summary>
/// <remarks>
/// <para>
/// Este error se utiliza cuando los datos de la solicitud son incorrectos o no cumplen
/// con las reglas de negocio. Típicamente se mapea a una respuesta HTTP 400 Bad Request.
/// </para>
/// <para>
/// Ejemplos de uso:
/// </para>
/// <list type="bullet">
/// <item><description>Categoría especificada no existe en el sistema</description></item>
/// <item><description>Nombre de producto duplicado</description></item>
/// <item><description>Datos con formato incorrecto</description></item>
/// <item><description>Violación de reglas de negocio específicas</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// if (!await _categoriaRepository.ExistsAsync(request.Categoria))
///     return new FunkoBadRequestError($"La categoría '{request.Categoria}' no existe");
/// 
/// if (await _funkoRepository.ExistsByNameAsync(request.Nombre))
///     return new FunkoBadRequestError($"Ya existe un producto con el nombre '{request.Nombre}'");
/// </code>
/// </example>
public record FunkoBadRequestError(
    /// <summary>
    /// Mensaje que describe por qué la solicitud es incorrecta.
    /// </summary>
    /// <example>
    /// "La categoría 'Marvel Comics' no existe", "Ya existe un producto con ese nombre"
    /// </example>
    string Error) : FunkoError(Error);

/// <summary>
/// Representa un error de validación de datos de productos Funko.
/// </summary>
/// <remarks>
/// <para>
/// Este error se utiliza cuando los datos no pasan las validaciones de la capa de servicio.
/// Típicamente se mapea a una respuesta HTTP 400 Bad Request.
/// </para>
/// <para>
/// Diferencia con <see cref="FunkoBadRequestError"/>:
/// </para>
/// <list type="bullet">
/// <item><description><strong>FunkoValidationError:</strong> Errores en la validación de formato, tipo o restricciones de datos</description></item>
/// <item><description><strong>FunkoBadRequestError:</strong> Errores de lógica de negocio (relaciones, duplicados, operaciones no permitidas)</description></item>
/// </list>
/// <para>
/// <strong>Consideración:</strong> Estos dos tipos de error son muy similares y podrían consolidarse
/// en uno solo para simplificar la jerarquía.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// if (request.Price &lt;= 0)
///     return new FunkoValidationError("El precio debe ser mayor que cero");
/// 
/// if (string.IsNullOrWhiteSpace(request.Nombre))
///     return new FunkoValidationError("El nombre del producto no puede estar vacío");
/// </code>
/// </example>
public record FunkoValidationError(
    /// <summary>
    /// Mensaje que describe la validación que falló.
    /// </summary>
    /// <example>
    /// "El precio debe ser positivo", "El nombre es demasiado largo"
    /// </example>
    string Error) : FunkoError(Error);

/// <summary>
/// Representa un error relacionado con el almacenamiento de productos Funko o sus archivos de imagen.
/// </summary>
/// <remarks>
/// <para>
/// Este error se utiliza cuando ocurre un problema durante operaciones de:
/// </para>
/// <list type="bullet">
/// <item><description>Base de datos (guardar, actualizar, eliminar productos)</description></item>
/// <item><description>Sistema de archivos (subir, eliminar imágenes de productos)</description></item>
/// <item><description>Almacenamiento externo (CDN, blob storage)</description></item>
/// </list>
/// <para>
/// Típicamente se mapea a una respuesta HTTP 500 Internal Server Error o 400 Bad Request
/// dependiendo del contexto.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo con error de base de datos:
/// <code>
/// try
/// {
///     await _repository.SaveAsync(funko);
/// }
/// catch (DbUpdateException ex)
/// {
///     _logger.LogError(ex, "Error al guardar el producto Funko");
///     return new FunkoStorageError("Error al guardar el producto en la base de datos");
/// }
/// </code>
/// 
/// Ejemplo con error de almacenamiento de archivos:
/// <code>
/// try
/// {
///     var imagePath = await _storageService.SaveImageAsync(file);
/// }
/// catch (IOException ex)
/// {
///     _logger.LogError(ex, "Error al guardar la imagen del producto");
///     return new FunkoStorageError("Error al guardar la imagen del producto");
/// }
/// </code>
/// </example>
public record FunkoStorageError(
    /// <summary>
    /// Mensaje que describe el error de almacenamiento o persistencia.
    /// </summary>
    /// <example>
    /// "Error al guardar la imagen del producto", "Error de conexión con la base de datos"
    /// </example>
    string Error) : FunkoError(Error);