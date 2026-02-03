using System.ComponentModel.DataAnnotations;
using FunkoApi.Dto.Users;
using NUnit.Framework;

namespace TestFunko.unit.dtos;

/// <summary>
/// Unit tests for User DTOs Data Annotations validation.
/// Verifies fields like email format, required fields, and name constraints.
/// </summary>
[TestFixture]
public class UserDtoTests
{
    #region RegisterDto Tests

    [Test]
    public void RegisterDto_WithFullValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "valid_user",
            Email = "valid@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Empty, "RegisterDto with valid data should pass validation");
    }

    [Test]
    public void RegisterDto_WhenUsernameHasInvalidCharacters_ShouldHaveValidationError()
    {
        // Arrange: Spaces are not allowed by regex
        var dto = new RegisterDto
        {
            Username = "user with space",
            Email = "valid@example.com",
            Password = "Password123"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("Solo se permiten letras, números y guiones bajos"));
    }

    [Test]
    public void RegisterDto_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "validuser",
            Email = "not-an-email",
            Password = "Password123"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("correo electrónico válido"));
    }

    [Test]
    public void RegisterDto_WhenPasswordIsTooShort_ShouldHaveValidationError()
    {
        // Arrange: 5 chars (min is 6)
        var dto = new RegisterDto
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "12345"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults, Is.Not.Empty);
        Assert.That(validationResults[0].ErrorMessage, Does.Contain("al menos 6 caracteres"));
    }

    #endregion

    #region LoginDto Tests

    [Test]
    public void LoginDto_WhenEmpty_ShouldFailForBothFields()
    {
        // Arrange
        var dto = new LoginDto();

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.That(validationResults.Count, Is.EqualTo(2), "Username and Password are required");
    }

    #endregion

    #region Value Preservation Tests (UserDto, AuthResponseDto)

    [Test]
    public void UserDto_ShouldPreserveValuesFromConstructor()
    {
        // Arrange
        var id = 100L;
        var username = "tester";
        var email = "test@test.com";
        var role = "USER";
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new UserDto(id, username, email, role, createdAt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(id));
            Assert.That(dto.Username, Is.EqualTo(username));
            Assert.That(dto.Email, Is.EqualTo(email));
            Assert.That(dto.Role, Is.EqualTo(role));
            Assert.That(dto.CreatedAt, Is.EqualTo(createdAt));
        });
    }

    [Test]
    public void AuthResponseDto_ShouldPreserveValuesFromConstructor()
    {
        // Arrange
        var token = "some.jwt.token";
        var user = new UserDto(1, "u", "e@e.com", "R", DateTime.UtcNow);

        // Act
        var dto = new AuthResponseDto(token, user);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.Token, Is.EqualTo(token));
            Assert.That(dto.User, Is.EqualTo(user));
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
