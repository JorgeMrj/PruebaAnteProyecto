using System.ComponentModel.DataAnnotations;

namespace FunkoApi.Dto.Funkasos;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para las solicitudes de creación y actualización de productos Funko.
/// </summary>
/// <remarks>
/// <para>
/// Este record se utiliza para validar y transportar los datos de entrada en las siguientes operaciones:
/// </para>
/// <list type="bullet">
/// <item><description>POST /api/funkos - Creación de un nuevo producto Funko</description></item>
/// <item><description>PUT /api/funkos/{id} - Actualización completa de un producto Funko existente</description></item>
/// </list>
/// <para>
/// Todas las propiedades (excepto Image) son obligatorias y se validan automáticamente
/// mediante Data Annotations antes de llegar a la lógica de negocio, gracias al atributo [ApiController].
/// </para>
/// <para>
/// <strong>Nota:</strong> Este DTO se usa en conjunto con archivos de imagen (IFormFile) 
/// en endpoints que aceptan multipart/form-data. El controlador recibe estos datos por separado
/// y los combina antes de enviarlos al servicio.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de objeto válido:
/// <code>
/// var funkoRequest = new FunkoRequestDto
/// {
///     Nombre = "Funko Pop Iron Man Mark 85",
///     Price = 29.99,
///     Categoria = "Marvel Avengers",
///     Image = null // Se establece por el servicio después de subir el archivo
/// };
/// </code>
/// 
/// Ejemplo de payload JSON (usado internamente, el controlador recibe multipart/form-data):
/// <code>
/// {
///   "nombre": "Funko Pop Batman Dark Knight",
///   "price": 34.99,
///   "categoria": "DC Comics",
///   "image": null
/// }
/// </code>
/// </example>
public record FunkoRequestDto
{
    /// <summary>
    /// Obtiene o establece el nombre del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto que describe el nombre completo del producto Funko.
    /// Debe tener entre 2 y 100 caracteres.
    /// </value>
    /// <remarks>
    /// Este campo es obligatorio y debe incluir información suficiente para identificar
    /// claramente el producto, incluyendo personaje, serie o edición especial si aplica.
    /// </remarks>
    /// <example>
    /// Valores válidos:
    /// <list type="bullet">
    /// <item><description>"Funko Pop Batman Dark Knight"</description></item>
    /// <item><description>"Spider-Man (No Way Home) #1109"</description></item>
    /// <item><description>"Baby Yoda (The Mandalorian)"</description></item>
    /// <item><description>"Iron Man Mark 85 (Avengers Endgame)"</description></item>
    /// </list>
    /// </example>
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el precio del producto Funko.
    /// </summary>
    /// <value>
    /// Un valor numérico de punto flotante que representa el precio del producto en la moneda base del sistema.
    /// Debe estar en el rango de 0.01 a 9999.99.
    /// </value>
    /// <remarks>
    /// El precio debe ser un valor positivo mayor a cero. Se permiten hasta dos decimales
    /// para representar centavos o céntimos.
    /// </remarks>
    /// <example>
    /// Valores válidos: 29.99, 15.00, 49.95, 199.99
    /// </example>
    [Required(ErrorMessage = "El precio del producto es obligatorio")]
    [Range(0.01, 9999.99, ErrorMessage = "El precio debe estar entre 0.01 y 9999.99")]
    public double Price { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador o nombre de la categoría a la que pertenece el producto.
    /// </summary>
    /// <value>
    /// Una cadena de texto que representa la categoría del producto Funko.
    /// Debe corresponder a una categoría existente en el sistema.
    /// </value>
    /// <remarks>
    /// <para>
    /// Este campo se utiliza para asociar el producto con una categoría existente.
    /// El valor debe coincidir con el nombre de una categoría previamente registrada.
    /// </para>
    /// <para>
    /// La validación de existencia de la categoría se realiza en la capa de servicio,
    /// no mediante Data Annotations.
    /// </para>
    /// </remarks>
    /// <example>
    /// Valores típicos: "Marvel Comics", "DC Comics", "Anime", "Star Wars", "Disney", "Harry Potter"
    /// </example>
    [Required(ErrorMessage = "La categoría del producto es obligatoria")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "La categoría debe tener entre 2 y 100 caracteres")]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta o URL de la imagen del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto opcional que representa la ruta relativa o absoluta del archivo de imagen.
    /// Puede ser null si aún no se ha asignado una imagen.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Importante:</strong> Este campo NO se envía directamente en las solicitudes del cliente.
    /// </para>
    /// <para>
    /// El flujo de trabajo es el siguiente:
    /// </para>
    /// <list type="number">
    /// <item><description>El cliente envía un IFormFile en el parámetro 'file' del controlador</description></item>
    /// <item><description>El controlador crea un FunkoRequestDto y lo pasa al servicio junto con el archivo</description></item>
    /// <item><description>El servicio guarda el archivo físico en el sistema de almacenamiento</description></item>
    /// <item><description>El servicio establece la propiedad Image con la ruta donde se guardó el archivo</description></item>
    /// <item><description>Finalmente, se guarda el producto en la base de datos con la ruta de la imagen</description></item>
    /// </list>
    /// <para>
    /// Si no se proporciona archivo de imagen, este campo permanecerá como null y el producto
    /// se guardará sin imagen asociada.
    /// </para>
    /// </remarks>
    /// <example>
    /// Valores típicos después del procesamiento:
    /// <list type="bullet">
    /// <item><description>"/uploads/funkos/batman-123456.jpg"</description></item>
    /// <item><description>"/images/funkos/2024/01/spiderman-789012.png"</description></item>
    /// <item><description>"https://cdn.ejemplo.com/funkos/ironman-345678.jpg"</description></item>
    /// <item><description>null (si no se subió imagen)</description></item>
    /// </list>
    /// </example>
    public string? Image { get; set; }
}