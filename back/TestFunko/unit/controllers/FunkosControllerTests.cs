using CSharpFunctionalExtensions;
using FunkoApi.Controllers;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;
using FunkoApi.Service.Funkos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.controllers;

/// <summary>
/// Unit tests for FunkosController following FIRST principles.
/// Verifies that all endpoints return the expected HTTP status codes and payloads.
/// </summary>
[TestFixture]
public class FunkosControllerTests
{
    private Mock<IFunkoService> _mockService;
    private FunkosController _controller;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IFunkoService>();
        _controller = new FunkosController(_mockService.Object);
    }

    #region Get Tests

    [Test]
    public async Task GetAsync_ShouldReturnOkWithList()
    {
        // Arrange
        var expectedFunkos = new List<FunkoResponseDto>
        {
            new(1L, "Funko 1", 10.0, "Cat 1", "img1.jpg"),
            new(2L, "Funko 2", 20.0, "Cat 2", "img2.jpg")
        };
        _mockService.Setup(s => s.GetFunkosAsync()).ReturnsAsync(expectedFunkos);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedFunkos));
    }

    [Test]
    public async Task GetAsync_ById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var response = new FunkoResponseDto(1L, "Funko", 10.0, "Cat", "img.jpg");
        _mockService.Setup(s => s.GetFunkoAsync(1L)).ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(response));

        // Act
        var result = await _controller.GetAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task GetAsync_ById_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetFunkoAsync(1L)).ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoNotFoundError("Not found")));

        // Act
        var result = await _controller.GetAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAsync_ById_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockService.Setup(s => s.GetFunkoAsync(1L)).ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoError("Internal error")));

        // Act
        var result = await _controller.GetAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = result as ObjectResult;
        Assert.That(objResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region Post Tests

    [Test]
    public async Task PostAsync_WhenValidData_ShouldReturnCreated()
    {
        // Arrange
        var name = "New Funko";
        var price = 15.0;
        var cat = "Marvel";
        var response = new FunkoResponseDto(1L, name, price, cat, "img.jpg");
        
        _mockService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(response));

        // Act
        var result = await _controller.PostAsync(name, price, cat, null);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = result as CreatedResult;
        Assert.That(createdResult!.Value, Is.EqualTo(response));
        Assert.That(createdResult.Location, Is.EqualTo($"/api/funkos/{response.Id}"));
    }

    [Test]
    public async Task PostAsync_WhenValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoValidationError("Invalid")));

        // Act
        var result = await _controller.PostAsync("a", 0, "b", null);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task PostAsync_WhenStorageError_ShouldReturnBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoStorageError("Storage fail")));

        // Act
        var result = await _controller.PostAsync("a", 10, "b", null);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task PostAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoError("Internal error")));

        // Act
        var result = await _controller.PostAsync("a", 10, "b", null);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = result as ObjectResult;
        Assert.That(objResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region Put Tests

    [Test]
    public async Task PutAsync_WhenValid_ShouldReturnOk()
    {
        // Arrange
        var response = new FunkoResponseDto(1L, "Updated", 20.0, "DC", "img.jpg");
        _mockService.Setup(s => s.UpdateFunkoAsync(It.IsAny<long>(), It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(response));

        // Act
        var result = await _controller.PutAsync(1L, "Updated", 20.0, "DC", null);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task PutAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.UpdateFunkoAsync(It.IsAny<long>(), It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoNotFoundError("Not found")));

        // Act
        var result = await _controller.PutAsync(1L, "Updated", 20.0, "DC", null);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task PutAsync_WhenValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.UpdateFunkoAsync(It.IsAny<long>(), It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoValidationError("Invalid")));

        // Act
        var result = await _controller.PutAsync(1L, "Updated", 20.0, "DC", null);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task PutAsync_WhenStorageError_ShouldReturnBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.UpdateFunkoAsync(It.IsAny<long>(), It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoStorageError("Storage Error")));

        // Act
        var result = await _controller.PutAsync(1L, "Updated", 20.0, "DC", null);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task PutAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockService.Setup(s => s.UpdateFunkoAsync(It.IsAny<long>(), It.IsAny<FunkoRequestDto>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoError("Internal error")));

        // Act
        var result = await _controller.PutAsync(1L, "Updated", 20.0, "DC", null);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = result as ObjectResult;
        Assert.That(objResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task DeleteAsync_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var response = new FunkoResponseDto(1L, "Deleted", 10.0, "Cat", "img.jpg");
        _mockService.Setup(s => s.DeleteFunkoAsync(1L)).ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(response));

        // Act
        var result = await _controller.DeleteAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteFunkoAsync(1L)).ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoNotFoundError("Not found")));

        // Act
        var result = await _controller.DeleteAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteAsync_WhenGenericError_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteFunkoAsync(1L)).ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoError("Internal error")));

        // Act
        var result = await _controller.DeleteAsync(1L);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = result as ObjectResult;
        Assert.That(objResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion
}
