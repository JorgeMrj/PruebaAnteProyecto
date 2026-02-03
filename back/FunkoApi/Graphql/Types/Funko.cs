using FunkoApi.Models;

namespace FunkoApi.Graphql.Types;

/// <summary>
/// Tipo de GraphQL para la entidad funko.
/// </summary>
public class FunkoType : ObjectType<Funko>
{
    protected override void Configure(IObjectTypeDescriptor<Funko> descriptor)
    {
        descriptor.Name("funko");
        descriptor.Description("Entidad funko");

        descriptor.Field(p => p.Id).Type<NonNullType<IdType>>().Description("El ID del funko");
        descriptor.Field(p => p.Name).Type<NonNullType<StringType>>().Description("El nombre del funko");
        descriptor.Field(p => p.Price).Type<NonNullType<DecimalType>>().Description("El precio del funko");
        descriptor.Field(p => p.Imagen).Type<StringType>().Description("URL de la imagen");
        descriptor.Field(p => p.CategoryId).Type<NonNullType<StringType>>().Description("El ID de la categoría");
        descriptor.Field(p => p.CreatedAt).Type<NonNullType<DateTimeType>>().Description("Fecha de creación");
        descriptor.Field(p => p.UpdatedAt).Type<NonNullType<DateTimeType>>().Description("Fecha de última actualización");

        descriptor.Field(p => p.Category)
            .Type<CategoryType>()
            .Description("La categoría del funko");
    }
}