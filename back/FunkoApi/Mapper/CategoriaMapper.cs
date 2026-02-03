using FunkoApi.Dto.Categories;
using FunkoApi.Models;

namespace FunkoApi.mapper;

/// <summary>
/// Clase estática para mapear entre modelos Categoria y sus DTOs.
/// </summary>
public static class CategoriaMapper
{
    /// <summary>
    /// Convierte un CategoriaRequestDto a un modelo Categoria.
    /// </summary>
    /// <param name="categoria">DTO con datos de creación/actualización.</param>
    /// <returns>Objeto Categoria con los datos del DTO.</returns>
    public static Categoria ToModel(this CategoriaRequestDto categoria)
    {
        return new Categoria
        {
            Nombre = categoria.Nombre,
        };
    }

    /// <summary>
    /// Convierte un modelo Categoria a un CategoriaResponseDto.
    /// </summary>
    /// <param name="categoria">Modelo de la categoría.</param>
    /// <returns>DTO de respuesta con datos de la categoría.</returns>
    public static CategoriaResponseDto ToDto(this Categoria categoria)
    {
        return new CategoriaResponseDto(categoria.Id, categoria.Nombre);
    }
}