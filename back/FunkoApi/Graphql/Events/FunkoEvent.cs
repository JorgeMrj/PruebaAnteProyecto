namespace FunkoApi.Graphql.Events;

/// <summary>
/// Evento publicado cuando se crea un nuevo funko.
/// </summary>
public record FunkoCreadoEvent
{
    public long FunkoId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public double Precio { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Evento publicado cuando se actualiza un funko.
/// </summary>
public record FunkoActualizadoEvent
{
    public long FunkoId { get; init; }
    public string? Nombre { get; init; }
    public double? Precio { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Evento publicado cuando se elimina un funko.
/// </summary>
public record FunkoEliminadoEvent
{
    public long FunkoId { get; init; }
    public DateTime DeletedAt { get; init; }
}

