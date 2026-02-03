using System.ComponentModel.DataAnnotations;

namespace FunkoApi.Dto.Categories;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para las solicitudes de creación y actualización de categorías.
/// </summary>
/// <remarks>
/// Este record se utiliza para validar y transportar los datos de entrada cuando se crea o actualiza una categoría.
/// Implementa validaciones mediante Data Annotations para garantizar la integridad de los datos.
/// Al ser un record, proporciona inmutabilidad por defecto y comparación por valor.
/// </remarks>
/// <example>
/// Ejemplo de uso en una solicitud JSON:
/// <code>
/// {
///   "nombre": "Categoría DC Comics"
/// }
/// </code>
/// </example>
public record CategoriaRequestDto
{
    /// <summary>
    /// Obtiene o establece el nombre de la categoría.
    /// </summary>
    /// <value>
    /// Una cadena que representa el nombre único de la categoría.
    /// Debe tener entre 2 y 100 caracteres.
    /// </value>
    /// <example>
    /// "Marvel", "DC Comics", "Anime", "Disney"
    /// </example>
    [Required(ErrorMessage = "Ingrese un nombre válido de categoría")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ingrese un nombre entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
}