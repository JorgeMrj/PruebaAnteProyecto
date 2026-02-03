using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunkoApi.Models;


/// <summary>
/// Representa una categoría de productos en el sistema.
/// </summary>
public record Categoria
{
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nombre de la categoría. Debe ser único y descriptivo.
    /// </summary>
    [Column]
    [Required]
    public string Nombre { get; set; }= string.Empty;
}