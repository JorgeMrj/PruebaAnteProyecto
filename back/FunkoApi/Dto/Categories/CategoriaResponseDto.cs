namespace FunkoApi.Dto.Categories;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) que representa la respuesta de una categoría.
/// </summary>
/// <remarks>
/// <para>
/// Este record se utiliza para serializar y devolver información de categorías desde la API hacia los clientes.
/// Se genera como respuesta en operaciones de consulta, creación, actualización y eliminación de categorías.
/// </para>
/// <para>
/// Utiliza el patrón de constructor posicional de records para inicialización concisa,
/// con propiedades mutables adicionales para compatibilidad con serialización JSON.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de respuesta JSON generada por este DTO:
/// <code>
/// {
///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "nombre": "Marvel Comics"
/// }
/// </code>
/// </example>
public record CategoriaResponseDto(
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    /// <value>
    /// Un GUID que identifica de forma única la categoría en el sistema.
    /// Este valor es generado automáticamente por la base de datos al crear la categoría.
    /// </value>
    /// <example>
    /// 3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    Guid Id,

    /// <summary>
    /// Nombre descriptivo de la categoría.
    /// </summary>
    /// <value>
    /// Una cadena de texto que representa el nombre de la categoría.
    /// </value>
    /// <example>
    /// "Marvel Comics", "DC Comics", "Anime", "Disney"
    /// </example>
    string Nombre)
{
    /// <summary>
    /// Obtiene o establece el identificador único de la categoría.
    /// </summary>
    /// <value>
    /// Un GUID que identifica de forma única la categoría en el sistema.
    /// </value>
    /// <remarks>
    /// Esta propiedad redeclara el parámetro posicional del constructor
    /// haciéndola mutable para compatibilidad con algunos serializadores y ORMs.
    /// </remarks>
    public Guid Id { get; set; } = Id;

    /// <summary>
    /// Obtiene o establece el nombre de la categoría.
    /// </summary>
    /// <value>
    /// Una cadena de texto que describe la categoría.
    /// </value>
    /// <remarks>
    /// Esta propiedad redeclara el parámetro posicional del constructor
    /// haciéndola mutable para compatibilidad con algunos serializadores y ORMs.
    /// </remarks>
    public string Nombre { get; set; } = Nombre;
}