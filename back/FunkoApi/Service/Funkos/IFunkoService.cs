using CSharpFunctionalExtensions;
using FunkoApi.Dto;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;

namespace FunkoApi.Service.Funkos;

/// <summary>
/// Interfaz que define los servicios lógicos para la gestión de productos Funko.
/// </summary>
public interface IFunkoService
{
    /// <summary>
    /// Obtiene todos los Funkos disponibles.
    /// </summary>
    /// <returns>Lista de DTOs de los Funkos.</returns>
    Task<List<FunkoResponseDto>> GetFunkosAsync();

    /// <summary>
    /// Obtiene un Funko por su ID.
    /// </summary>
    /// <param name="id">ID del Funko.</param>
    /// <returns>Result con el DTO del Funko o error.</returns>
    Task<Result<FunkoResponseDto,FunkoError>> GetFunkoAsync(long id);

    /// <summary>
    /// Guarda un nuevo Funko.
    /// </summary>
    /// <param name="request">Datos del Funko.</param>
    /// <param name="file">Archivo de imagen opcional.</param>
    /// <returns>Result con el DTO creado o error.</returns>
    Task<Result<FunkoResponseDto,FunkoError>> SaveFunkoAsync( FunkoRequestDto request,IFormFile? file);

    /// <summary>
    /// Elimina un Funko.
    /// </summary>
    /// <param name="id">ID del Funko.</param>
    /// <returns>Result con el DTO eliminado o error.</returns>
    Task<Result<FunkoResponseDto,FunkoError>> DeleteFunkoAsync(long id);

    /// <summary>
    /// Actualiza un Funko existente.
    /// </summary>
    /// <param name="id">ID del Funko.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <param name="file">Nueva imagen opcional.</param>
    /// <returns>Result con el DTO actualizado o error.</returns>
    Task<Result<FunkoResponseDto,FunkoError>> UpdateFunkoAsync(long id,FunkoRequestDto request,IFormFile? file);
}