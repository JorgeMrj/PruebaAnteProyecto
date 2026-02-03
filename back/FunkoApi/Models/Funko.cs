using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunkoApi.Models;

/// <summary>
/// Representa un producto Funko Pop en el sistema.
/// </summary>
public record Funko
{
    /// <summary>
    /// Nombre del archivo de imagen por defecto.
    /// </summary>
    public const string IMAGE_DEFAULT = "default.png";
    
    /// <summary>
    /// Identificador único del Funko. Generado automáticamente por la base de datos.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption. Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// Nombre del Funko.
    /// </summary>
    [Column]
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificador de la categoría a la que pertenece el Funko.
    /// </summary>
    [Column]
    [Required]
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Propiedad de navegación a la categoría relacionada.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Categoria? Category { get; set; }
    
    /// <summary>
    /// Nombre o ruta del archivo de imagen asociado al Funko.
    /// </summary>
    [Column]
    [Required]
    public string Imagen { get; set; } = IMAGE_DEFAULT;
    
    /// <summary>
    /// Precio del Funko.
    /// </summary>
    [Column]
    [Required]
    [Range(0, int.MaxValue)]
    public double Price { get; set; }
    
    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    [Column]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de la última actualización del registro.
    /// </summary>
    [Column]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}