using System.ComponentModel.DataAnnotations;

namespace FunkoApi.Dto.Funkasos; 

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para solicitudes de actualización parcial (PATCH) de productos Funko.
/// </summary>
/// <remarks>
/// <para>
/// Este DTO se utiliza en operaciones HTTP PATCH para permitir la actualización selectiva de campos
/// de un producto Funko sin necesidad de enviar todos los datos.
/// </para>
/// <para>
/// A diferencia de PUT (que reemplaza todo el recurso), PATCH permite actualizar solo los campos
/// que se incluyan en la solicitud, dejando los demás sin modificar.
/// </para>
/// <para>
/// <strong>Nota importante:</strong> Actualmente no hay un endpoint PATCH implementado en el controlador.
/// Este DTO está preparado para una futura implementación de actualización parcial.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de payload JSON para actualizar solo el precio:
/// <code>
/// {
///   "price": 39.99
/// }
/// </code>
/// 
/// Ejemplo para actualizar nombre y categoría:
/// <code>
/// {
///   "nombre": "Funko Pop Iron Man Mark 85",
///   "categoria": "Marvel Avengers"
/// }
/// </code>
/// </example>
public class FunkoPatchRequestDto
{
    /// <summary>
    /// Obtiene o establece el nombre del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto opcional que representa el nuevo nombre del producto.
    /// Si se proporciona, debe tener entre 2 y 100 caracteres.
    /// Si no se incluye en la solicitud, el nombre actual del producto no se modificará.
    /// </value>
    /// <example>
    /// "Funko Pop Batman", "Spider-Man (Spider-Verse)", "Baby Yoda (The Mandalorian)"
    /// </example>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string? Nombre { get; set; } = null;

    /// <summary>
    /// Obtiene o establece el precio del producto Funko.
    /// </summary>
    /// <value>
    /// Un valor numérico decimal opcional que representa el nuevo precio del producto en la moneda base del sistema.
    /// Debe estar en el rango de 0.01 a 9999.99.
    /// Si es null, el precio actual del producto no se modificará.
    /// </value>
    /// <example>
    /// 29.99, 49.95, 15.00
    /// </example>
    [Range(0.01, 9999.99, ErrorMessage = "El precio debe estar entre 0.01 y 9999.99")]
    public double? Price { get; set; } = null;

    /// <summary>
    /// Obtiene o establece la categoría del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto opcional que representa el identificador o nombre de la nueva categoría.
    /// Si no se incluye en la solicitud, la categoría actual del producto no se modificará.
    /// </value>
    /// <example>
    /// "Marvel Comics", "DC Comics", "Anime", "Star Wars"
    /// </example>
    public string? Categoria { get; set; } = null;

    /// <summary>
    /// Obtiene o establece la URL o ruta de la imagen del producto Funko.
    /// </summary>
    /// <value>
    /// Una cadena de texto opcional que representa la ruta relativa o URL de la nueva imagen del producto.
    /// Si es null o no se incluye, la imagen actual del producto no se modificará.
    /// </value>
    /// <remarks>
    /// <para>
    /// Este campo normalmente contiene la ruta del archivo de imagen previamente subido,
    /// no los datos binarios de la imagen.
    /// </para>
    /// <para>
    /// Para subir una nueva imagen, se debe usar el endpoint con multipart/form-data
    /// que está disponible en PUT /api/funkos/{id}.
    /// </para>
    /// </remarks>
    /// <example>
    /// "/images/funkos/batman-dark-knight.jpg", "https://cdn.example.com/funkos/123.png"
    /// </example>
    public string? Image { get; set; } = null;
}