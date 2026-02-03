using CSharpFunctionalExtensions;
using FunkoApi.Dto.Funkasos;
using FunkoApi.Error;
using FunkoApi.Graphql.Inputs.FunkosInputs;
using FunkoApi.Graphql.Mutations;
using FunkoApi.Service.Funkos;
using HotChocolate;
using Moq;
using NUnit.Framework;

namespace TestFunko.unit.graphql;

/// <summary>
/// Unit tests for FunkoMutation (GraphQL).
/// Verifies that GraphQL mutations correctly delegate to IFunkoService and handle results/errors.
/// </summary>
[TestFixture]
public class FunkoMutationsTests
{
    private Mock<IFunkoService> _mockFunkoService;
    private FunkoMutation _mutation;

    [SetUp]
    public void Setup()
    {
        _mockFunkoService = new Mock<IFunkoService>();
        _mutation = new FunkoMutation(_mockFunkoService.Object);
    }

    #region Create Funko

    [Test]
    public async Task Createfunko_WhenSuccess_ShouldReturnResponseDto()
    {
        // Arrange
        var input = new CreateFunkoInput { Nombre = "New Funko", Precio = 10.0, CategoriaId = "cat1" };
        var expectedResponse = new FunkoResponseDto(1L, "New Funko", 10.0, "cat1", "default.png");
        
        _mockFunkoService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), null))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(expectedResponse));

        // Act
        var result = await _mutation.Createfunko(input, _mockFunkoService.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        _mockFunkoService.Verify(s => s.SaveFunkoAsync(
            It.Is<FunkoRequestDto>(d => d.Nombre == input.Nombre && d.Price == input.Precio), 
            null), Times.Once);
    }

    [Test]
    public void Createfunko_WhenFailure_ShouldThrowGraphQLException()
    {
        // Arrange
        var input = new CreateFunkoInput { Nombre = "Error" };
        _mockFunkoService.Setup(s => s.SaveFunkoAsync(It.IsAny<FunkoRequestDto>(), null))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoBadRequestError("Invalid data")));

        // Act & Assert
        var ex = Assert.ThrowsAsync<GraphQLException>(() => _mutation.Createfunko(input, _mockFunkoService.Object));
        Assert.That(ex.Errors[0].Message, Is.EqualTo("Invalid data"));
    }

    #endregion

    #region Update Funko

    [Test]
    public async Task UpdateFunko_WhenSuccess_ShouldReturnUpdatedResponseDto()
    {
        // Arrange
        var id = 1L;
        var input = new UpdateFunkoInput { Nombre = "Updated Name" };
        var existing = new FunkoResponseDto(id, "Old Name", 10.0, "cat1", "img.png");
        var updated = new FunkoResponseDto(id, "Updated Name", 10.0, "cat1", "img.png");

        _mockFunkoService.Setup(s => s.GetFunkoAsync(id))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(existing));
        _mockFunkoService.Setup(s => s.UpdateFunkoAsync(id, It.IsAny<FunkoRequestDto>(), null))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(updated));

        // Act
        var result = await _mutation.UpdateFunko(id, input, _mockFunkoService.Object);

        // Assert
        Assert.That(result, Is.EqualTo(updated));
        _mockFunkoService.Verify(s => s.UpdateFunkoAsync(id, It.Is<FunkoRequestDto>(d => d.Nombre == "Updated Name"), null), Times.Once);
    }

    [Test]
    public void UpdateFunko_WhenFunkoNotFound_ShouldThrowGraphQLException()
    {
        // Arrange
        var id = 99L;
        _mockFunkoService.Setup(s => s.GetFunkoAsync(id))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoNotFoundError("id no encontrada: "+id)));

        // Act & Assert
        Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateFunko(id, new UpdateFunkoInput(), _mockFunkoService.Object));
    }

    #endregion

    #region Delete Funko

    [Test]
    public async Task DeleteFunko_WhenSuccess_ShouldReturnDeletedResponseDto()
    {
        // Arrange
        var id = 1L;
        var deleted = new FunkoResponseDto(id, "Deleted", 0, "", "");
        _mockFunkoService.Setup(s => s.DeleteFunkoAsync(id))
            .ReturnsAsync(Result.Success<FunkoResponseDto, FunkoError>(deleted));

        // Act
        var result = await _mutation.DeleteFunko(id, _mockFunkoService.Object);

        // Assert
        Assert.That(result, Is.EqualTo(deleted));
        _mockFunkoService.Verify(s => s.DeleteFunkoAsync(id), Times.Once);
    }

    [Test]
    public void DeleteFunko_WhenFailure_ShouldThrowGraphQLException()
    {
        // Arrange
        var id = 1L;
        _mockFunkoService.Setup(s => s.DeleteFunkoAsync(id))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, FunkoError>(new FunkoNotFoundError("id no encontrada: "+id)));

        // Act & Assert
        Assert.ThrowsAsync<GraphQLException>(() => _mutation.DeleteFunko(id, _mockFunkoService.Object));
    }

    #endregion
}
