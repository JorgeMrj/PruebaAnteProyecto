using FunkoApi.Models;

namespace FunkoApi.Graphql.Types;

/// <summary>
/// Tipo de GraphQL para la entidad Categoria.
/// </summary>
public class CategoryType : ObjectType<Categoria>
{
    protected override void Configure(IObjectTypeDescriptor<Categoria> descriptor)
    {
        descriptor.Name("Categoria");
        descriptor.Description("Entidad Categoria");

        descriptor.Field(c => c.Id).Type<NonNullType<IdType>>().Description("El ID de la categoría");
        descriptor.Field(c => c.Nombre).Type<NonNullType<StringType>>().Description("El nombre de la categoría");
    }
}