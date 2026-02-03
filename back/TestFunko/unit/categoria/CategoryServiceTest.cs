using FunkoApi.Dto.Categories;
using FunkoApi.Error;
using FunkoApi.Handler.Categorias;
using FunkoApi.Models;
using FunkoApi.Repository.Category;
using FunkoApi.Service.Cache;
using FunkoApi.Service.Category;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestFunko.unit.categoria;

[TestFixture]
public class CategoriaServiceTest
{
    private Mock<ICategoryRepository> _repositoryMock;
    private Mock<ICacheService> _cacheMock;
    private Mock<ILogger<CategoriaService>> _loggerMock;
    private Mock<CategoriaWebSocketHandler> _webSocketMock;
    private CategoriaService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CategoriaService>>();
        
        _webSocketMock = new Mock<CategoriaWebSocketHandler>(Mock.Of<ILogger<CategoriaWebSocketHandler>>());

        _service = new CategoriaService(
            _loggerMock.Object,
            _repositoryMock.Object,
            _cacheMock.Object,
            _webSocketMock.Object
        );
    }

    [Test]
    public async Task GetCategoriaAsync_WhenIdExistsInCache_ShouldReturnCachedValue()
    {
        // Arrange: Preparamos una categoría en el "cache"
        var id = Guid.NewGuid().ToString();
        var categoriaEnCache = new Categoria { Id = Guid.Parse(id), Nombre = "Cache Test" };
        
        _cacheMock.Setup(c => c.GetAsync<Categoria>(It.IsAny<string>()))
            .ReturnsAsync(categoriaEnCache);

        // Act: Llamamos al servicio
        var result = await _service.GetCategoriaAsync(id);

        // Assert: Verificamos que sea éxito y venga del cache (no llamó al repo)
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Cache Test"));
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetCategoriaAsync_WhenIdNotFoundAnywhere_ShouldReturnNotFoundError()
    {
        // Arrange: Ni cache ni repo tienen la categoría
        var id = "no-existe";
        _cacheMock.Setup(c => c.GetAsync<Categoria>(It.IsAny<string>()))
            .ReturnsAsync((Categoria)null!);
        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Categoria)null!);

        // Act
        var result = await _service.GetCategoriaAsync(id);

        // Assert: Debe fallar con un error de tipo CategoriaNotFoundError
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<CategoriaNotFoundError>());
    }

    [Test]
    public async Task SaveCategoriaAsync_WhenSuccess_ShouldNotifyViaWebSocket()
    {
        // Arrange: Datos de entrada y mock del guardado
        var request = new CategoriaRequestDto { Nombre = "Nueva" };
        var categoriaGuardada = new Categoria { Id = Guid.NewGuid(), Nombre = "Nueva" };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Categoria>()))
            .ReturnsAsync(categoriaGuardada);

        // Act
        var result = await _service.SaveCategoriaAsync(request);

        // Assert: Verificamos éxito y que se intentó notificar (WebSocket)
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Nueva"));
        
        // Verificamos que se haya llamado al repositorio una vez
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Categoria>(c => c.Nombre == "Nueva")), Times.Once);
    }

    [Test]
    public async Task DeleteCategoriaAsync_WhenIdExists_ShouldRemoveFromCacheAndReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoriaEliminada = new Categoria { Id = id, Nombre = "AEliminar" };

        _repositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(categoriaEliminada);

        // Act
        var result = await _service.DeleteCategoriaAsync(id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        // Verificamos que se limpie el cache usando el nombre (según lógica de tu Service)
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(s => s.Contains("AEliminar"))), Times.Once);
    }

    [Test]
    public async Task UpdateCategoriaAsync_WhenIdDoesNotExist_ShouldReturnFailure()
    {
        // Arrange: El repo devuelve null al intentar actualizar
        var id = Guid.NewGuid();
        var request = new CategoriaRequestDto { Nombre = "Editada" };

        _repositoryMock.Setup(r => r.UpdateAsync(id, It.IsAny<Categoria>()))
            .ReturnsAsync((Categoria)null!);

        // Act
        var result = await _service.UpdateCategoriaAsync(id, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}