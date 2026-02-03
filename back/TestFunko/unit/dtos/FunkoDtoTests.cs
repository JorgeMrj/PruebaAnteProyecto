using System.ComponentModel.DataAnnotations;
using FunkoApi.Dto.Funkasos;
using NUnit.Framework;

namespace TestFunko.unit.dtos;

/// <summary>
/// Unit tests for Funko DTOs Data Annotations validation.
/// Uses Validator.TryValidateObject to ensure constraints are enforced.
/// </summary>
[TestFixture]
public class FunkoDtoTests
{
    #region FunkoRequestDto Tests

    [Test]
    public void FunkoRequestDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new FunkoRequestDto
        {
            Nombre = "Iron Man Mark 85",
            Price = 29.99,
            Categoria = "Marvel"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Empty, "DTO with valid data should not have validation errors");
    }

    [Test]
    public void FunkoRequestDto_WhenNombreIsTooShort_ShouldHaveValidationError()
    {
        // Arrange: Name with only 1 character (min is 2)
        var dto = new FunkoRequestDto
        {
            Nombre = "A",
            Price = 25.00,
            Categoria = "Test"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("2 y 100 caracteres"));
    }

    [Test]
    public void FunkoRequestDto_WhenPriceIsOutOfRange_ShouldHaveValidationError()
    {
        // Arrange: Price below 0.01
        var dto = new FunkoRequestDto
        {
            Nombre = "Valid Name",
            Price = 0.00,
            Categoria = "Test"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("precio debe estar entre 0.01 y 9999.99"));
    }

    [Test]
    public void FunkoRequestDto_WhenRequiredFieldsAreMissing_ShouldHaveValidationErrors()
    {
        // Arrange: Empty DTO
        var dto = new FunkoRequestDto();

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults.Count, Is.GreaterThanOrEqualTo(3), "Should fail for Nombre, Price (if 0 and required), and Categoria");
    }

    #endregion

    #region FunkoPatchRequestDto Tests

    [Test]
    public void FunkoPatchRequestDto_WithPartialValidData_ShouldPassValidation()
    {
        // Arrange: Only price updated
        var dto = new FunkoPatchRequestDto
        {
            Price = 45.00
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Empty, "Partial update with valid data should pass");
    }

    [Test]
    public void FunkoPatchRequestDto_WhenNombreIsTooShortInPatch_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new FunkoPatchRequestDto
        {
            Nombre = "X"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("2 y 100 caracteres"));
    }

    #endregion

    #region FunkoResponseDto Tests

    [Test]
    public void FunkoResponseDto_ShouldPreserveValuesFromConstructor()
    {
        // Arrange
        var id = 42L;
        var name = "Groot";
        var price = 15.50;
        var category = "Guardians";
        var image = "groot.png";

        // Act
        var dto = new FunkoResponseDto(id, name, price, category, image);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(id));
            Assert.That(dto.Nombre, Is.EqualTo(name));
            Assert.That(dto.Precio, Is.EqualTo(price));
            Assert.That(dto.Categoria, Is.EqualTo(category));
            Assert.That(dto.Imagen, Is.EqualTo(image));
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
