using FunkoApi.Dto;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Models;

namespace FunkoApi.mapper;

/// <summary>
/// Clase estática para mapear entre modelos Funko y sus DTOs.
/// </summary>
public static class FunkosMapper
{
    /// <summary>
    /// Convierte un FunkoRequestDto a un modelo Funko.
    /// </summary>
    /// <param name="dto">DTO con datos del producto.</param>
    /// <param name="categoria">Categoría asociada al producto.</param>
    /// <returns>Modelo Funko poblado.</returns>
    public static Funko ToModel(this FunkoRequestDto dto, Categoria categoria )
    {
        return new Funko
        {
            Name = dto.Nombre,
            Category = categoria,
            Price = dto.Price,
            Imagen = dto.Image ?? Funko.IMAGE_DEFAULT
        };


    }

    /// <summary>
    /// Convierte un modelo Funko a un FunkoResponseDto.
    /// </summary>
    /// <param name="funko">Modelo del producto.</param>
    /// <returns>DTO de respuesta con datos del producto.</returns>
    public static FunkoResponseDto ToDto(this Funko funko)
    {
        return new FunkoResponseDto(
            funko.Id,
            funko.Name, 
            funko.Price, 
            funko.Category!.Nombre,
            funko.Imagen
        );
    }
}