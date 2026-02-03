using FunkoApi.Graphql.Queries;
using FunkoApi.Models;
using FunkoApi.Repository.Category;
using FunkoApi.Repository.funkos;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.graphql;

/// <summary>
/// Unit tests for FunkoQuery (GraphQL).
/// Verifies that the GraphQL query methods correctly delegate to the underlying repositories.
/// </summary>
[TestFixture]
public class FunkoQueriesTests
{
    private Mock<IFunkoRepository> _mockFunkoRepository;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private FunkoQuery _query;

    [SetUp]
    public void Setup()
    {
        _mockFunkoRepository = new Mock<IFunkoRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _query = new FunkoQuery();
    }

    #region Funko Queries

    [Test]
    public void GetFunkos_ShouldCallRepositoryFindAll()
    {
        // Arrange
        var expectedFunkos = new List<Funko>().AsQueryable();
        _mockFunkoRepository.Setup(r => r.FindAllAsNoTracking()).Returns(expectedFunkos);

        // Act
        var result = _query.GetFunkos(_mockFunkoRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedFunkos));
        _mockFunkoRepository.Verify(r => r.FindAllAsNoTracking(), Times.Once);
    }

    [Test]
    public async Task GetFunko_ShouldCallRepositoryGetById()
    {
        // Arrange
        var id = 1L;
        var expectedFunko = new Funko { Id = id, Name = "Test Funko" };
        _mockFunkoRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expectedFunko);

        // Act
        var result = await _query.GetFunko(id, _mockFunkoRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedFunko));
        _mockFunkoRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Test]
    public void GetFunkosPaged_ShouldCallRepositoryFindAll()
    {
        // Arrange
        var expectedFunkos = new List<Funko>().AsQueryable();
        _mockFunkoRepository.Setup(r => r.FindAllAsNoTracking()).Returns(expectedFunkos);

        // Act
        var result = _query.GetFunkosPaged(_mockFunkoRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedFunkos));
        _mockFunkoRepository.Verify(r => r.FindAllAsNoTracking(), Times.Once);
    }

    #endregion

    #region Category Queries

    [Test]
    public void GetCategorias_ShouldCallRepositoryFindAll()
    {
        // Arrange
        var expectedCategorias = new List<Categoria>().AsQueryable();
        _mockCategoryRepository.Setup(r => r.FindAllAsNoTracking()).Returns(expectedCategorias);

        // Act
        var result = _query.GetCategorias(_mockCategoryRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCategorias));
        _mockCategoryRepository.Verify(r => r.FindAllAsNoTracking(), Times.Once);
    }

    [Test]
    public async Task GetCategoria_ShouldCallRepositoryGetById()
    {
        // Arrange
        var id = "some-id";
        var expectedCategoria = new Categoria { Id = Guid.NewGuid(), Nombre = "Test Category" };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expectedCategoria);

        // Act
        var result = await _query.GetCategoria(id, _mockCategoryRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCategoria));
        _mockCategoryRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Test]
    public void GetCategoriasPaged_ShouldCallRepositoryFindAll()
    {
        // Arrange
        var expectedCategorias = new List<Categoria>().AsQueryable();
        _mockCategoryRepository.Setup(r => r.FindAllAsNoTracking()).Returns(expectedCategorias);

        // Act
        var result = _query.GetCategoriasPaged(_mockCategoryRepository.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCategorias));
        _mockCategoryRepository.Verify(r => r.FindAllAsNoTracking(), Times.Once);
    }

    #endregion
}
