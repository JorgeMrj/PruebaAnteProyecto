namespace FunkoApi.Dto.Funkasos;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) que representa la respuesta de un producto Funko.
/// </summary>
/// <remarks>
/// <para>
/// Este record se utiliza para serializar y devolver información de productos Funko desde la API hacia los clientes.
/// Se genera como respuesta en las siguientes operaciones:
/// </para>
/// <list type="bullet">
/// <item><description>GET /api/funkos - Lista de todos los productos Funko</description></item>
/// <item><description>GET /api/funkos/{id} - Consulta de un producto específico</description></item>
/// <item><description>POST /api/funkos - Confirmación de creación de nuevo producto</description></item>
/// <item><description>PUT /api/funkos/{id} - Confirmación de actualización de producto</description></item>
/// <item><description>DELETE /api/funkos/{id} - Confirmación de eliminación de producto</description></item>
/// </list>
/// <para>
/// <strong>Nota de diseño:</strong> Este record utiliza campos en lugar de propiedades auto-implementadas,
/// lo cual es inusual y genera código redundante. Considere usar un record posicional puro para mayor simplicidad.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de respuesta JSON generada por este DTO:
/// <code>
/// {
///   "id": 123,
///   "nombre": "Funko Pop Batman Dark Knight",
///   "precio": 29.99,
///   "categoria": "DC Comics",
///   "imagen": "/uploads/funkos/batman-123456.jpg"
/// }
/// </code>
/// 
/// Ejemplo de uso en código:
/// <code>
/// var funko = new FunkoResponseDto(
///     Id: 123,
///     Nombre: "Funko Pop Iron Man",
///     Precio: 29.99,
///     Categoria: "Marvel Comics",
///     Imagen: "/uploads/funkos/ironman.jpg"
/// );
/// </code>
/// </example>
public record FunkoResponseDto(
    /// <summary>
    /// Identificador único del producto Funko.
    /// </summary>
    /// <value>
    /// Un número entero largo (long) que identifica de forma única el producto en el sistema.
    /// Este valor es generado automáticamente por la base de datos al crear el producto.
    /// </value>
    /// <example>
    /// 123, 456, 789
    /// </example>
    long Id,

    /// <summary>
    /// Nombre descriptivo del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto que describe el nombre completo del producto,
    /// incluyendo personaje, serie o edición especial si aplica.
    /// </value>
    /// <example>
    /// "Funko Pop Batman Dark Knight", "Spider-Man (No Way Home) #1109", "Baby Yoda (The Mandalorian)"
    /// </example>
    string Nombre,

    /// <summary>
    /// Precio del producto Funko.
    /// </summary>
    /// <value>
    /// Un valor numérico de punto flotante que representa el precio del producto en la moneda base del sistema.
    /// </value>
    /// <remarks>
    /// <strong>Advertencia:</strong> Se utiliza <see langword="double"/> para el precio, 
    /// lo cual puede causar problemas de precisión en cálculos monetarios.
    /// Se recomienda migrar a <see cref="decimal"/> en futuras versiones.
    /// </remarks>
    /// <example>
    /// 29.99, 15.00, 49.95, 199.99
    /// </example>
    double Precio,

    /// <summary>
    /// Categoría a la que pertenece el producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto que representa el nombre de la categoría del producto.
    /// </value>
    /// <example>
    /// "Marvel Comics", "DC Comics", "Anime", "Star Wars", "Disney"
    /// </example>
    string Categoria,

    /// <summary>
    /// Ruta o URL de la imagen del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto que representa la ruta relativa o URL absoluta del archivo de imagen asociado al producto.
    /// </value>
    /// <remarks>
    /// Esta ruta apunta al archivo de imagen que fue subido durante la creación o actualización del producto.
    /// Si el producto no tiene imagen asociada, este campo puede contener una cadena vacía o una ruta por defecto.
    /// </remarks>
    /// <example>
    /// "/uploads/funkos/batman-123456.jpg", "https://cdn.ejemplo.com/funkos/ironman.png"
    /// </example>
    string Imagen);