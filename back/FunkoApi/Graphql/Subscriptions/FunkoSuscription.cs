using FunkoApi.Graphql.Events;
using HotChocolate.Authorization;

namespace FunkoApi.Graphql.Subscriptions;

/// <summary>
/// Suscripciones GraphQL para eventos de funkos en tiempo real.
/// </summary>
public class FunkoSubscription
{
    /// <summary>Evento cuando se crea un funko.</summary>
    /// <param name="message">Datos del evento.</param>
    /// <returns>Evento publicado.</returns>
    [Authorize]
    [Subscribe]
    [Topic]
    public FunkoCreadoEvent OnFunkoCreado([EventMessage] FunkoCreadoEvent message) => message;

    /// <summary>Evento cuando se actualiza un funko.</summary>
    /// <param name="message">Datos del evento.</param>
    /// <returns>Evento publicado.</returns>
    [Authorize]
    [Subscribe]
    [Topic]
    public FunkoActualizadoEvent OnFunkoActualizado([EventMessage] FunkoActualizadoEvent message) => message;

    /// <summary>Evento cuando se elimina un funko.</summary>
    /// <param name="message">Datos del evento.</param>
    /// <returns>Evento publicado.</returns>
    [Authorize]
    [Subscribe]
    [Topic]
    public FunkoEliminadoEvent OnFunkoEliminado([EventMessage] FunkoEliminadoEvent message) => message;

}