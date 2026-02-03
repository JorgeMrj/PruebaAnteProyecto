using CSharpFunctionalExtensions;
using FunkoApi.Controllers;
using FunkoApi.Dto.Categories;
using FunkoApi.Error;
using FunkoApi.Service.Category;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.controllers;

/// <summary>
/// Unit tests for CategoriaController following FIRST principles.
/// Verifies that controller actions return the expected HTTP status codes for all success and failure paths.
/// </summary>
[TestFixture]
public class CategoriaControllerTests
{
    private Mock<ICategoriaService> _mockService;
    private CategoriaController _controller;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<ICategoriaService>();
        _controller = new CategoriaController(_mockService.Object);
    }

    #region GET Tests

    [Test]
    public async Task GetAsync_ShouldReturnOkWithListOfCategories()
    {
        // Arrange
        var expected = new List<CategoriaResponseDto>
        {
            new CategoriaResponseDto(Guid.NewGuid(), "Cat 1")
        };
        _mockService.Setup(s => s.GetCategoriasAsync()).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetAsync_ById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var idString = Guid.NewGuid().ToString();
        var idGuid = Guid.Parse(idString);
        var expected = new CategoriaResponseDto(idGuid, "Cat 1");
        _mockService.Setup(s => s.GetCategoriaAsync(idString))
            .ReturnsAsync(Result.Success<CategoriaResponseDto, CategoriaError>(expected));

        // Act
        var result = await _controller.GetAsync(idString);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetAsync_ById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var id = "missing-id";
        _mockService.Setup(s => s.GetCategoriaAsync(id))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaNotFoundError("not found")));

        // Act
        var result = await _controller.GetAsync(id);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAsync_ById_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var id = "id";
        _mockService.Setup(s => s.GetCategoriaAsync(id))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaStorageError("boom")));

        // Act
        var result = await _controller.GetAsync(id);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region POST Tests

    [Test]
    public async Task PostAsync_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        var request = new CategoriaRequestDto { Nombre = "New" };
        var response = new CategoriaResponseDto(Guid.NewGuid(), "New");
        _mockService.Setup(s => s.SaveCategoriaAsync(request))
            .ReturnsAsync(Result.Success<CategoriaResponseDto, CategoriaError>(response));

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = result as CreatedResult;
        Assert.That(createdResult!.Value, Is.EqualTo(response));
        Assert.That(createdResult.Location, Is.EqualTo($"/api/categoria/{response.Id}"));
    }

    [Test]
    public async Task PostAsync_WhenBadRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CategoriaRequestDto();
        _mockService.Setup(s => s.SaveCategoriaAsync(request))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaBadRequestError("fail")));

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task PostAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CategoriaRequestDto();
        _mockService.Setup(s => s.SaveCategoriaAsync(request))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaStorageError("boom")));

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region PUT Tests

    [Test]
    public async Task PutAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CategoriaRequestDto { Nombre = "Updated" };
        var response = new CategoriaResponseDto(id, "Updated");
        _mockService.Setup(s => s.UpdateCategoriaAsync(id, request))
            .ReturnsAsync(Result.Success<CategoriaResponseDto, CategoriaError>(response));

        // Act
        var result = await _controller.PutAsync(id, request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task PutAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CategoriaRequestDto();
        _mockService.Setup(s => s.UpdateCategoriaAsync(id, request))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaNotFoundError("not found")));

        // Act
        var result = await _controller.PutAsync(id, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task PutAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CategoriaRequestDto();
        _mockService.Setup(s => s.UpdateCategoriaAsync(id, request))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaStorageError("boom")));

        // Act
        var result = await _controller.PutAsync(id, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region DELETE Tests

    [Test]
    public async Task DeleteAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CategoriaResponseDto(id, "Deleted");
        _mockService.Setup(s => s.DeleteCategoriaAsync(id))
            .ReturnsAsync(Result.Success<CategoriaResponseDto, CategoriaError>(response));

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteCategoriaAsync(id))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaNotFoundError("not found")));

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteCategoriaAsync(id))
            .ReturnsAsync(Result.Failure<CategoriaResponseDto, CategoriaError>(new CategoriaStorageError("boom")));

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion
}
