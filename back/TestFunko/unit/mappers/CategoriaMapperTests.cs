using NUnit.Framework;
using FunkoApi.Dto.Categories;
using FunkoApi.mapper;
using FunkoApi.Models;

namespace TestFunko.unit.mappers;

/// <summary>
/// Unit tests for CategoriaMapper following FIRST principles.
/// These tests verify the mapping between Categoria model and its DTOs.
/// </summary>
[TestFixture]
public class CategoriaMapperTests
{
    [Test]
    public void ToModel_ShouldMapRequestDtoToCategoriaModelCorrectly()
    {
        // Arrange: Create a request DTO with a specific name
        var dto = new CategoriaRequestDto
        {
            Nombre = "Disney"
        };

        // Act: Use the mapper extension method to convert to model
        var model = dto.ToModel();

        // Assert: Verify the name was correctly mapped
        Assert.That(model.Nombre, Is.EqualTo(dto.Nombre), "The name in the model should match the DTO");
    }

    [Test]
    public void ToDto_ShouldMapCategoriaModelToResponseDtoCorrectly()
    {
        // Arrange: Create a Categoria model with ID and name
        var guid = Guid.NewGuid();
        var model = new Categoria
        {
            Id = guid,
            Nombre = "Marvel"
        };

        // Act: Convert model to response DTO
        var responseDto = model.ToDto();

        // Assert: Verify both ID and Name are correct
        Assert.Multiple(() =>
        {
            Assert.That(responseDto.Id, Is.EqualTo(model.Id), "The ID should be preserved in the DTO");
            Assert.That(responseDto.Nombre, Is.EqualTo(model.Nombre), "The name should be preserved in the DTO");
        });
    }
}
