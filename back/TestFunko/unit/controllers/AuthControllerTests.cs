using CSharpFunctionalExtensions;
using FunkoApi.Controllers;
using FunkoApi.Dto.Users;
using FunkoApi.Error;
using FunkoApi.Service.auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.controllers;

/// <summary>
/// Unit tests for AuthController following FIRST principles.
/// Verifies that authentication actions return the expected HTTP status codes for all success and failure scenarios.
/// </summary>
[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<ILogger<AuthController>> _mockLogger;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    #region SignUp Tests

    [Test]
    public async Task SignUp_WhenSuccess_ShouldReturnCreated()
    {
        // Arrange
        var userDto = new UserDto(1L, "user", "test@test.com", "ADMIN", DateTime.UtcNow);
        var dto = new RegisterDto{Email ="test@test.com",Password="Password123!",Username = "user"};
        var response = new AuthResponseDto("token", userDto);
        _mockAuthService.Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Success<AuthResponseDto, AuthError>(response));

        // Act
        var result = await _controller.SignUp(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult!.Value, Is.EqualTo(response));
        Assert.That(createdResult.ActionName, Is.EqualTo("SignUp"));
    }

    [Test]
    public async Task SignUp_WhenValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new RegisterDto{Password="",Username="", Email = ""};
        _mockAuthService.Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new ValidationError("Invalid data")));

        // Act
        var result = await _controller.SignUp(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SignUp_WhenConflictError_ShouldReturnConflict()
    {
        // Arrange
        var dto = new RegisterDto{Username="duplicate", Email="test@test.com", Password = "Password123!"};
        _mockAuthService.Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new ConflictError("User already exists")));

        // Act
        var result = await _controller.SignUp(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task SignUp_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var dto = new RegisterDto{Username="error", Email="test@test.com", Password="Password123!"};
        _mockAuthService.Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new AuthError("Internal error")));

        // Act
        var result = await _controller.SignUp(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region SignIn Tests

    [Test]
    public async Task SignIn_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var userDto = new UserDto(1L, "user", "test@test.com", "ADMIN", DateTime.UtcNow);
        var dto = new LoginDto{Password="password",Username="user"};
        var response = new AuthResponseDto("token", userDto);
        _mockAuthService.Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Success<AuthResponseDto, AuthError>(response));

        // Act
        var result = await _controller.SignIn(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task SignIn_WhenUnauthorizedError_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new LoginDto{Username="wrong", Password="password"};
        _mockAuthService.Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new UnauthorizedError("Invalid credentials")));

        // Act
        var result = await _controller.SignIn(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task SignIn_WhenValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new LoginDto{Username="", Password= ""};
        _mockAuthService.Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new ValidationError("Missing fields")));

        // Act
        var result = await _controller.SignIn(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SignIn_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var dto = new LoginDto{Username="error", Password="password"};
        _mockAuthService.Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, AuthError>(new AuthError("Internal error")));

        // Act
        var result = await _controller.SignIn(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion
}
