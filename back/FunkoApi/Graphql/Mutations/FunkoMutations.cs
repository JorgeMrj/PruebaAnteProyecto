using CSharpFunctionalExtensions;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;
using FunkoApi.Graphql.Inputs.FunkosInputs;
using FunkoApi.Service.Funkos;
using HotChocolate.Authorization;

namespace FunkoApi.Graphql.Mutations;

/// <summary>
/// Mutations de GraphQL para funkos (requiere rol ADMIN).
/// </summary>
public class FunkoMutation
{
    private readonly IFunkoService _funkoService;

    /// <summary>Constructor para tests.</summary>
    public FunkoMutation(IFunkoService funkoService) => _funkoService = funkoService;

    /// <summary>Crea un nuevo funko.</summary>
    /// <param name="input">Datos del funko.</param>
    /// <param name="service">Servicio de funkos.</param>
    /// <returns>funko creado o error.</returns>
    [Authorize(policy: "AdminOnly")]
    public async Task<FunkoResponseDto> Createfunko(
        CreateFunkoInput input,
        [Service] IFunkoService service)
    {
        var dto = new FunkoRequestDto
        {
            Nombre = input.Nombre,
            Price = input.Precio,
            Categoria = input.CategoriaId,
            Image = input.Imagen,
        };
        var result = await service.SaveFunkoAsync(dto, null);
        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Error);
    }

    /// <summary>Actualiza un funko existente.</summary>
    /// <param name="id">ID del funko.</param>
    /// <param name="input">Campos a modificar.</param>
    /// <param name="service">Servicio de funkos.</param>
    /// <returns>funko actualizado o error.</returns>
    [Authorize(policy: "AdminOnly")]
    public async Task<FunkoResponseDto> UpdateFunko(
        long id,
        UpdateFunkoInput input,
        [Service] IFunkoService service)
    {
        var existingResult = await service.GetFunkoAsync(id);
        if (existingResult.IsFailure)
            throw new GraphQLException(existingResult.Error.Error);

        var dto = new FunkoRequestDto
        {
            Nombre = input.Nombre ?? existingResult.Value.Nombre,
            Price = input.Precio ?? existingResult.Value.Precio,
            Categoria = input.CategoriaId ?? existingResult.Value.Categoria,
            Image = input.Imagen ?? existingResult.Value.Imagen,
        };
        var updated=await service.UpdateFunkoAsync(id, dto,null);
        return updated.IsSuccess
            ? updated.Value
            : throw new GraphQLException(updated.Error.Error);
    }

    /// <summary>Elimina un funko (soft delete).</summary>
    /// <param name="id">ID del funko.</param>
    /// <param name="service">Servicio de funkos.</param>
    /// <returns>Éxito o error.</returns>
    [Authorize(policy: "AdminOnly")]
    public async Task<FunkoResponseDto> DeleteFunko(
        long id,
        [Service] IFunkoService service)
    {
        var result= await service.DeleteFunkoAsync(id);
        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Error.Error);
    }

    
}
