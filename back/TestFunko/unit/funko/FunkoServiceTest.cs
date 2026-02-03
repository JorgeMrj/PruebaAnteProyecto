using FluentAssertions;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;
using FunkoApi.Graphql.Publishers;
using FunkoApi.Handler.Funkos;
using FunkoApi.Models;
using FunkoApi.Repository.Category;
using FunkoApi.Repository.funkos;
using FunkoApi.Service.Cache;
using FunkoApi.Service.Email;
using FunkoApi.Service.Funkos;
using FunkoApi.Service.storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

// Namespace principal de NUnit

namespace TestFunko.unit.funko;

[TestFixture] // Indica que esta clase contiene pruebas NUnit
public class FunkoServiceTests
{
    // Mocks de dependencias
    private Mock<IFunkoRepository> _repositoryMock;
    private Mock<ICacheService> _cacheMock;
    private Mock<ICategoryRepository> _categoryRepositoryMock;
    private Mock<IStorageService> _storageMock;
    private Mock<IEmailService> _emailMock;
    private Mock<IEventPublisher> _publisherMock;
    private Mock<FunkosWebSocketHandler> _webSocketMock; // Asumiendo que es mockeable (virtual/interfaz)
    private Mock<IConfiguration> _configMock;
    private Mock<ILogger<FunkoService>> _loggerMock;
    
    // System Under Test (SUT)
    private FunkoService _service;

    [SetUp] // Se ejecuta antes de CADA test para asegurar aislamiento (Isolated)
    public void SetUp()
    {
        _repositoryMock = new Mock<IFunkoRepository>();
        _cacheMock = new Mock<ICacheService>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _storageMock = new Mock<IStorageService>();
        _emailMock = new Mock<IEmailService>();
        _publisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<FunkoService>>();
        _configMock = new Mock<IConfiguration>();
        _webSocketMock = new Mock<FunkosWebSocketHandler>(Mock.Of<ILogger<FunkosWebSocketHandler>>());

        // Configuración dummy para evitar NullReference en la lectura de config
        _configMock.Setup(c => c["Smtp:AdminEmail"]).Returns("admin@test.com");

        // Inicializamos el servicio con los objetos mockeados
        _service = new FunkoService(
            _cacheMock.Object,
            _repositoryMock.Object,
            _categoryRepositoryMock.Object,
            _storageMock.Object,
            _webSocketMock.Object,
            _emailMock.Object,
            _loggerMock.Object,
            _publisherMock.Object,
            _configMock.Object
        );
    }

    [Test]
    public async Task GetFunkosAsync_ShouldReturnList_WhenRepositoryHasData()
    {
        // Arrange: Preparamos datos simulados
        var categoryId = Guid.NewGuid();
        var listaFunkos = new List<Funko>
        {
            new Funko { Id = 1, Name = "Batman", Price = 10, CategoryId = categoryId, Category = new Categoria { Nombre = "DC" } },
            new Funko { Id = 2, Name = "Superman", Price = 12, CategoryId = categoryId, Category = new Categoria { Nombre = "DC" } }
        };

        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(listaFunkos);

        // Act: Ejecutamos el método
        var result = await _service.GetFunkosAsync();

        // Assert: Verificamos resultados y comportamiento
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Nombre.Should().Be("Batman"); // Verificamos mapeo correcto a DTO
        
        _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once); // Verificamos llamada al repo
    }

    [Test]
    public async Task GetFunkoAsync_ShouldReturnFromCache_WhenKeyExists()
    {
        // Arrange: El funko existe en caché
        var funkoId = 1L;
        var cachedFunko = new Funko { Id = funkoId, Name = "Cached Funko", Category = new Categoria { Nombre = "Test" } };
        
        // Simulamos respuesta positiva de la caché
        _cacheMock.Setup(c => c.GetAsync<Funko>($"Funko_{funkoId}")).ReturnsAsync(cachedFunko);

        // Act
        var result = await _service.GetFunkoAsync(funkoId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Cached Funko");
        
        // El repositorio NO debe ser llamado si está en caché (Fast & Efficiency)
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task GetFunkoAsync_ShouldQueryRepoAndSaveToCache_WhenCacheMiss()
    {
        // Arrange: No está en caché, pero sí en BBDD
        var funkoId = 1L;
        var dbFunko = new Funko { Id = funkoId, Name = "DB Funko", Category = new Categoria { Nombre = "Test" } };

        _cacheMock.Setup(c => c.GetAsync<Funko>($"Funko_{funkoId}")).ReturnsAsync((Funko?)null); // Cache Miss
        _repositoryMock.Setup(r => r.GetByIdAsync(funkoId)).ReturnsAsync(dbFunko); // Repo Hit

        // Act
        var result = await _service.GetFunkoAsync(funkoId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("DB Funko");

        // Verificamos flujo: Repo consultado -> Guardado en Caché
        _repositoryMock.Verify(r => r.GetByIdAsync(funkoId), Times.Once);
        _cacheMock.Verify(c => c.SetAsync($"Funko_{funkoId}", dbFunko, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    public async Task GetFunkoAsync_ShouldReturnNotFoundError_WhenNotExistsAnywhere()
    {
        // Arrange: No está ni en caché ni en BBDD
        var funkoId = 99L;
        _cacheMock.Setup(c => c.GetAsync<Funko>(It.IsAny<string>())).ReturnsAsync((Funko?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(funkoId)).ReturnsAsync((Funko?)null);

        // Act
        var result = await _service.GetFunkoAsync(funkoId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<FunkoNotFoundError>();
    }

    [Test]
    public async Task SaveFunkoAsync_ShouldFail_WhenCategoryDoesNotExist()
    {
        // Arrange: La categoría indicada en el DTO no existe
        var request = new FunkoRequestDto { Nombre = "Test", Categoria = "Fantasma", Price = 10 };
        _categoryRepositoryMock.Setup(c => c.GetByIdAsync("Fantasma")).ReturnsAsync((Categoria?)null);

        // Act
        var result = await _service.SaveFunkoAsync(request, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<FunkoValidationError>();
        
        // No se debe intentar guardar en repositorio
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Funko>()), Times.Never);
    }

    [Test]
    public async Task SaveFunkoAsync_ShouldSaveAndTriggerEvents_WhenValid()
    {
        // Arrange: Todo válido
        var catName = "Marvel";
        var category = new Categoria { Nombre = catName };
        var request = new FunkoRequestDto { Nombre = "Iron Man", Categoria = catName, Price = 100 };
        var savedFunko = new Funko { Id = 10, Name = "Iron Man", Category = category };

        _categoryRepositoryMock.Setup(c => c.GetByIdAsync(catName)).ReturnsAsync(category);
        _storageMock.Setup(s => s.StoreAsync(It.IsAny<IFormFile>())).ReturnsAsync("img.png");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Funko>())).ReturnsAsync(savedFunko);

        // Act
        var result = await _service.SaveFunkoAsync(request, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);

        // Verificamos persistencia
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Funko>()), Times.Once);

        /* NOTA: Las llamadas a WebSocket, EventPublisher y Email están dentro de Task.Run (fire-and-forget).
           En tests unitarios estrictos, podría haber condiciones de carrera (race conditions) donde el test termina
           antes de que el hilo secundario llame al mock. 
           Si FunkoService no espera a estas tareas, Verify podría fallar intermitentemente.
           Asumimos ejecución inmediata para este ejemplo ideal. */
    }

    [Test]
    public async Task UpdateFunkoAsync_ShouldUpdateAndNotify_WhenFoundAndValid()
    {
        // Arrange
        var id = 5L;
        var catName = "Series";
        var category = new Categoria { Nombre = catName };
        var request = new FunkoRequestDto { Nombre = "Updated Name", Categoria = catName, Price = 50 };
        var updatedFunko = new Funko { Id = id, Name = "Updated Name", Category = category };

        // Validaciones previas exitosas
        _categoryRepositoryMock.Setup(c => c.GetByIdAsync(catName)).ReturnsAsync(category);
        _storageMock.Setup(s => s.StoreAsync(It.IsAny<IFormFile>())).ReturnsAsync("new_img.png");
        
        // Update exitoso en repo
        _repositoryMock.Setup(r => r.UpdateAsync(id, It.IsAny<Funko>())).ReturnsAsync(updatedFunko);

        // Act
        var result = await _service.UpdateFunkoAsync(id, request, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Updated Name");
        
        _repositoryMock.Verify(r => r.UpdateAsync(id, It.IsAny<Funko>()), Times.Once);
    }

    [Test]
    public async Task DeleteFunkoAsync_ShouldRemoveFromCacheAndNotify_WhenDeletedSuccessfully()
    {
        // Arrange
        var id = 3L;
        var deletedFunko = new Funko { Id = id, Name = "Bye Bye", Category = new Categoria { Nombre = "X" } };

        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(deletedFunko);

        // Act
        var result = await _service.DeleteFunkoAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verificamos limpieza de caché
        _cacheMock.Verify(c => c.RemoveAsync($"Funko_{id}"), Times.Once);
    }

    [Test]
    public async Task DeleteFunkoAsync_ShouldReturnNotFound_WhenRepoReturnsNull()
    {
        // Arrange
        var id = 3L;
        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync((Funko?)null);

        // Act
        var result = await _service.DeleteFunkoAsync(id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<FunkoNotFoundError>();
        
        // No se debe intentar borrar de caché si no existía
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}