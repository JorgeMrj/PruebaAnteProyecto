using FunkoApi.config;
using FunkoApi.Dto.Users;
using FunkoApi.Repository.Users;
using FunkoApi.Service.auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestFunko.integracion;

/// <summary>
/// Integration tests for AuthService using real database with Testcontainers.
/// </summary>
[TestFixture]
[NonParallelizable]
public class UserServiceIntegrationTests
{
    private PostgreSqlContainer _postgresContainer;
    private IServiceProvider _serviceProvider;
    private FunkoDbContext _dbContext;
    private IAuthService _authService;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("funko_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _postgresContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    [SetUp]
    public async Task Setup()
    {
        var connectionString = _postgresContainer.GetConnectionString();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString },
                { "Jwt:Key", "SuperSecretKeyForTestingAuthServiceIntegrationInFunkoApi" },
                { "Jwt:Issuer", "FunkoApi" },
                { "Jwt:Audience", "FunkoApi" },
                { "Jwt:ExpireMinutes", "60" }
            }!)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());

        services.AddDbContext<FunkoDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        _serviceProvider = services.BuildServiceProvider();

        _dbContext = _serviceProvider.GetRequiredService<FunkoDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();

        _authService = _serviceProvider.GetRequiredService<IAuthService>();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }

        if (_serviceProvider is IDisposable sp)
        {
            sp.Dispose();
        }
    }

    #region SignUpAsync Integration Tests

    [Test]
    public async Task SignUpAsync_WhenNewUser_ShouldPersistInDatabase()
    {
        // Arrange
        var dto = new RegisterDto{Username="newuser", Email="test@test.com", Password="Password123!"};

        // Act
        var result = await _authService.SignUpAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.User.Username, Is.EqualTo("newuser"));
        
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb.Email, Is.EqualTo("test@test.com"));
    }

    [Test]
    public async Task SignUpAsync_WhenUsernameExists_ShouldReturnConflict()
    {
        // Arrange
        var dto1 = new RegisterDto{Username="existing", Email="test1@test.com", Password="Password123!"};
        await _authService.SignUpAsync(dto1);
        
        var dto2 = new RegisterDto{Username="existing", Email="test2@test.com", Password="Password123!"};

        // Act
        var result = await _authService.SignUpAsync(dto2);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<FunkoApi.Error.ConflictError>());
    }

    #endregion

    #region SignInAsync Integration Tests

    [Test]
    public async Task SignInAsync_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var registerDto = new RegisterDto{Username="loginuser", Email="login@test.com", Password="Secret123!"};
        await _authService.SignUpAsync(registerDto);
        
        var loginDto = new LoginDto{Username="loginuser", Password="Secret123!"};

        // Act
        var result = await _authService.SignInAsync(loginDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.Not.Null.Or.Empty);
    }

    [Test]
    public async Task SignInAsync_WhenPasswordIsWrong_ShouldReturnUnauthorized()
    {
        // Arrange
        await _authService.SignUpAsync(new RegisterDto{Username="user", Email="user@test.com", Password="Correct123!"});
        var loginDto = new LoginDto{Username="user", Password="Wrong123!"};

        // Act
        var result = await _authService.SignInAsync(loginDto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<FunkoApi.Error.UnauthorizedError>());
    }

    #endregion
}