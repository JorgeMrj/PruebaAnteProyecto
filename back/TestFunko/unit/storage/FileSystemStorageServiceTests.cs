using System.Text;
using FunkoApi.config;
using FunkoApi.exception;
using FunkoApi.Service.storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace TestFunko.unit.storage;

[TestFixture]
public class FileSystemStorageServiceTests
{
    private Mock<ILogger<FileSystemStorageService>> _loggerMock;
    private IOptions<StorageSettings> _options;
    private FileSystemStorageService _storageService;
    private string _testRootPath;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<FileSystemStorageService>>();
        
        // Configuramos settings de prueba
        var _settings = new StorageSettings
        {
            RootPath = "TestUploads",
            ImagesFolder = "images",
            DocumentsFolder = "docs",
            MaxFileSize = 1024 * 1024, // 1MB para el test
            AllowedExtensions = new[] { ".jpg", ".png", ".txt" },
            DeleteOnStartup = false
        };

        _options = Options.Create(_settings);
        _storageService = new FileSystemStorageService(_options, _loggerMock.Object);
        
        // Obtenemos la ruta real donde el servicio trabajará
        _testRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestUploads"));
    }

    [TearDown]
    public void TearDown()
    {
        // Limpieza de archivos físicos creados durante los tests
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
    }

    [Test]
    public async Task InitAsync_ShouldCreateRequiredDirectories()
    {
        // Act: Inicializamos el servicio
        await _storageService.InitAsync();

        // Assert: Verificamos que se crearon las carpetas definidas en settings
        Assert.That(Directory.Exists(_testRootPath), Is.True);
        Assert.That(Directory.Exists(Path.Combine(_testRootPath, "images")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(_testRootPath, "temp")), Is.True);
    }

    [Test]
    public async Task StoreAsync_WithValidFile_ShouldSaveSuccessfullyAndReturnUniqueName()
    {
        // Arrange: Creamos un archivo de texto válido
        var content = "Contenido de prueba";
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Callback<Stream, CancellationToken>((s, c) => stream.CopyTo(s));

        // Act
        var generatedName = await _storageService.StoreAsync(fileMock.Object, "docs");

        // Assert
        Assert.That(generatedName, Does.Contain(".txt"));
        Assert.That(generatedName, Does.Not.EqualTo(fileName)); // Debe ser único (trae timestamp/guid)
        
        var fullPath = _storageService.GetFilePath(generatedName, "docs");
        Assert.That(File.Exists(fullPath), Is.True);
    }

    [Test]
    public void StoreAsync_WhenFileExceedsMaxSize_ShouldThrowFileSizeExceededException()
    {
        // Arrange: Archivo de 2MB (el límite es 1MB en Setup)
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(2 * 1024 * 1024);
        fileMock.Setup(f => f.FileName).Returns("heavy.jpg");

        // Act & Assert
        Assert.ThrowsAsync<FileSizeExceededException>(async () => 
            await _storageService.StoreAsync(fileMock.Object));
    }

    [Test]
    public void StoreAsync_WhenExtensionNotAllowed_ShouldThrowInvalidFileTypeException()
    {
        // Arrange: Extensión .exe no está en la lista blanca
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("virus.exe");
        fileMock.Setup(f => f.Length).Returns(100);

        // Act & Assert
        Assert.ThrowsAsync<InvalidFileTypeException>(async () => 
            await _storageService.StoreAsync(fileMock.Object));
    }

    [Test]
    public async Task DeleteAsync_WhenFileExists_ShouldReturnTrueAndRemoveFile()
    {
        // Arrange: Creamos un archivo manualmente en el sistema
        await _storageService.InitAsync();
        var folder = "temp";
        var fileName = "to-delete.txt";
        var path = Path.Combine(_testRootPath, folder, fileName);
        File.WriteAllText(path, "borrame");

        // Act
        var result = await _storageService.DeleteAsync(fileName, folder);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(path), Is.False);
    }

    [Test]
    public async Task DeleteAsync_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _storageService.DeleteAsync("inventado.jpg");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetUrl_ShouldNormalizeSlashesForWeb()
    {
        // Act
        var url = _storageService.GetUrl("image.jpg", "products");

        // Assert: Incluso en Windows debe devolver "/"
        Assert.That(url, Is.EqualTo("/uploads/products/image.jpg"));
        Assert.That(url, Does.Not.Contain("\\"));
    }

    [Test]
    public async Task ListFilesAsync_ShouldReturnFileNamesInFolder()
    {
        // Arrange
        await _storageService.InitAsync();
        var folder = "images";
        File.WriteAllText(Path.Combine(_testRootPath, folder, "img1.jpg"), "...");
        File.WriteAllText(Path.Combine(_testRootPath, folder, "img2.jpg"), "...");

        // Act
        var files = await _storageService.ListFilesAsync(folder);

        // Assert
        Assert.That(files, Has.Exactly(2).Items);
        Assert.That(files, Contains.Item("img1.jpg"));
        Assert.That(files, Contains.Item("img2.jpg"));
    }
}