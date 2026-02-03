using System.ComponentModel.DataAnnotations;
using FunkoApi.Dto.Categories;
using NUnit.Framework;

namespace TestFunko.unit.dtos;

/// <summary>
/// Unit tests for Category DTOs Data Annotations validation.
/// Verifies that required fields and length constraints are enforced.
/// </summary>
[TestFixture]
public class CategoriaDtoTests
{
    #region CategoriaRequestDto Tests

    [Test]
    public void CategoriaRequestDto_WithValidName_ShouldPassValidation()
    {
        // Arrange
        var dto = new CategoriaRequestDto
        {
            Nombre = "Disney"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Empty, "DTO with valid name should not have validation errors");
    }

    [Test]
    public void CategoriaRequestDto_WhenNombreIsTooShort_ShouldHaveValidationError()
    {
        // Arrange: Name with only 1 character (min is 2)
        var dto = new CategoriaRequestDto
        {
            Nombre = "A"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("entre 2 y 100 caracteres"));
    }

    [Test]
    public void CategoriaRequestDto_WhenNombreIsMissing_ShouldHaveValidationError()
    {
        // Arrange: Empty name
        var dto = new CategoriaRequestDto
        {
            Nombre = ""
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("nombre válido de categoría"));
    }

    #endregion

    #region CategoriaResponseDto Tests

    [Test]
    public void CategoriaResponseDto_ShouldPreserveValuesFromConstructor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Marvel Comics";

        // Act
        var dto = new CategoriaResponseDto(id, name);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(id), "ID should match constructor argument");
            Assert.That(dto.Nombre, Is.EqualTo(name), "Name should match constructor argument");
        });
    }

    #endregion

    #region Helper Methods

    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    #endregion
}
