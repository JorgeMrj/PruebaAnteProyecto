using FunkoApi.Controllers;
using FunkoApi.Service.storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.IO;

namespace TestFunko.unit.controllers;

[TestFixture]
public class StorageControllerTests
{
    private Mock<IStorageService> _mockStorageService;
    private Mock<ILogger<FilesController>> _mockLogger;
    private FilesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockStorageService = new Mock<IStorageService>();
        _mockLogger = new Mock<ILogger<FilesController>>();
        _controller = new FilesController(_mockStorageService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Download_WhenFileExists_ShouldReturnFileResult()
    {
        // Arrange
        var fileName = "test.png";
        var folder = "uploads";
        var stream = new MemoryStream();
        _mockStorageService.Setup(s => s.LoadAsStreamAsync(fileName, folder, default))
            .ReturnsAsync(stream);

        // Act
        var result = await _controller.Download(fileName, folder);

        // Assert
        Assert.That(result, Is.InstanceOf<FileStreamResult>());
    }

    [Test]
    public async Task Download_WhenFileNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var fileName = "missing.txt";
        _mockStorageService.Setup(s => s.LoadAsStreamAsync(fileName, It.IsAny<string>(), default))
            .ThrowsAsync(new System.IO.FileNotFoundException());

        // Act
        var result = await _controller.Download(fileName);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #region GetUrl Tests

    [Test]
    public void GetUrl_WhenFileExists_ShouldReturnOkWithUrl()
    {
        // Arrange
        var fileName = "img.jpg";
        var folder = "images";
        var expectedUrl = "http://localhost/images/img.jpg";
        _mockStorageService.Setup(s => s.Exists(fileName, folder)).Returns(true);
        _mockStorageService.Setup(s => s.GetUrl(fileName, folder)).Returns(expectedUrl);

        // Act
        var result = _controller.GetUrl(fileName, folder);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as FilesController.FileUrlResponse;
        Assert.That(response!.FileName, Is.EqualTo(fileName));
        Assert.That(response.Url, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void GetUrl_WhenFileDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var fileName = "no-image.jpg";
        _mockStorageService.Setup(s => s.Exists(fileName, It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.GetUrl(fileName);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    #endregion

    #region ServeFile Tests

    [Test]
    public void ServeFile_WhenFileDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var path = "nonexistent/path.png";
        _mockStorageService.Setup(s => s.GetFilePath(path, It.IsAny<string>())).Returns("dummy_path");

        // Act
        var result = _controller.ServeFile(path);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    #endregion

    #region ContentType Tests

    [Test]
    [TestCase("test.jpg", "image/jpeg")]
    [TestCase("test.png", "image/png")]
    [TestCase("test.pdf", "application/pdf")]
    [TestCase("test.xlsx", "application/octet-stream")]
    public async Task GetContentType_ShouldReturnCorrectMimeType(string fileName, string expectedMimeType)
    {
        // Arrange
        _mockStorageService.Setup(s => s.LoadAsStreamAsync(fileName, It.IsAny<string>(), default))
            .ReturnsAsync(new MemoryStream());

        // Act
        var result = await _controller.Download(fileName) as FileStreamResult;

        // Assert
        Assert.That(result!.ContentType, Is.EqualTo(expectedMimeType));
    }

    #endregion
}
