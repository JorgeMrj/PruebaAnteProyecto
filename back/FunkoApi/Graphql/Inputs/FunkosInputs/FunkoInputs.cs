namespace FunkoApi.Graphql.Inputs.FunkosInputs;

/// <summary>
    /// Datos de entrada para crear un funko.
    /// </summary>
    public record CreateFunkoInput
{
    /// <summary>
    /// Nombre del funko. Obligatorio y único.
    /// </summary>
    /// <example>Laptop Dell XPS 15</example>
    public string Nombre { get; init; } = string.Empty;


    /// <summary>
    /// Precio del funko. Debe ser mayor a 0.
    /// </summary>
    /// <example>1299.99</example>
    public double Precio { get; init; }


    /// <summary>
    /// URL de la imagen. Opcional, debe ser URL válida.
    /// </summary>
    /// <example>https://ejemplo.com/imagen.jpg</example>
    public string? Imagen { get; init; }

    /// <summary>
    /// ID de la categoría. Obligatorio y debe existir.
    /// </summary>
    /// <example>1</example>
    public string CategoriaId { get; init; }=string.Empty;
}

/// <summary>
    /// Datos de entrada para actualizar un funko.
    /// </summary>
    public record UpdateFunkoInput
{
    /// <summary>
    /// Nuevo nombre del funko (opcional).
    /// Si es null, no se modifica el nombre actual.
    /// </summary>
    /// <example>Laptop Dell XPS 15 Actualizado</example>
    public string? Nombre { get; init; }


    /// <summary>
    /// Nuevo precio (opcional).
    /// Si es null, no se modifica el precio actual.
    /// Debe ser mayor a 0 si se proporciona.
    /// </summary>
    /// <example>1199.99</example>
    public double? Precio { get; init; }


    /// <summary>
    /// Nueva URL de imagen (opcional).
    /// Si es null, no se modifica la imagen actual.
    /// </summary>
    /// <example>https://ejemplo.com/nueva-imagen.jpg</example>
    public string? Imagen { get; init; }

    /// <summary>
    /// Nuevo ID de categoría (opcional).
    /// Si es null, no se modifica la categoría actual.
    /// </summary>
    /// <example>2</example>
    public string? CategoriaId { get; init; }
}