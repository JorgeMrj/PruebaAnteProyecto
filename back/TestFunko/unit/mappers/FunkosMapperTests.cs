using FunkoApi.Dto.Funkasos;
using FunkoApi.mapper;
using FunkoApi.Models;

namespace TestFunko.unit.mappers;

/// <summary>
/// Unit tests for FunkosMapper following FIRST principles.
/// These tests verify the mapping between models and DTOs.
/// </summary>
[TestFixture]
public class FunkosMapperTests
{
    [Test]
    public void ToModel_ShouldMapRequestDtoToFunkoModelCorrectly()
    {
        // Arrange: Create a DTO and a category
        var category = new Categoria { Id = Guid.NewGuid(), Nombre = "Marvel" };
        var dto = new FunkoRequestDto
        {
            Nombre = "Iron Man",
            Price = 25.99,
            Categoria = "Marvel",
            Image = "ironman.png"
        };

        // Act: Map the DTO to a model
        var model = dto.ToModel(category);

        // Assert: Verify all properties were mapped correctly
        Assert.Multiple(() =>
        {
            Assert.That(model.Name, Is.EqualTo(dto.Nombre), "Name should match");
            Assert.That(model.Price, Is.EqualTo(dto.Price), "Price should match");
            Assert.That(model.Imagen, Is.EqualTo(dto.Image), "Image should match");
            Assert.That(model.Category, Is.EqualTo(category), "Category should be assigned correctly");
        });
    }

    [Test]
    public void ToModel_WhenImageIsNullInDto_ShouldUseDefaultImage()
    {
        // Arrange: DTO with null image
        var category = new Categoria { Id = Guid.NewGuid(), Nombre = "Marvel" };
        var dto = new FunkoRequestDto
        {
            Nombre = "Iron Man",
            Price = 25.99,
            Categoria = "Marvel",
            Image = null
        };

        // Act
        var model = dto.ToModel(category);

        // Assert
        Assert.That(model.Imagen, Is.EqualTo(Funko.IMAGE_DEFAULT), "Should use default image if DTO image is null");
    }

    [Test]
    public void ToDto_ShouldMapFunkoModelToResponseDtoCorrectly()
    {
        // Arrange: Create a model with category
        var category = new Categoria { Id = Guid.NewGuid(), Nombre = "Star Wars" };
        var model = new Funko
        {
            Id = 1L,
            Name = "Darth Vader",
            Price = 29.99,
            Category = category,
            Imagen = "vader.png"
        };

        // Act: Map the model to a DTO
        var responseDto = model.ToDto();

        // Assert: Verify all fields in the response DTO
        Assert.Multiple(() =>
        {
            Assert.That(responseDto.Id, Is.EqualTo(model.Id), "ID should match");
            Assert.That(responseDto.Nombre, Is.EqualTo(model.Name), "Nombre should match");
            Assert.That(responseDto.Precio, Is.EqualTo(model.Price), "Precio should match");
            Assert.That(responseDto.Categoria, Is.EqualTo(category.Nombre), "Categoria name should match");
            Assert.That(responseDto.Imagen, Is.EqualTo(model.Imagen), "Imagen should match");
        });
    }
}
