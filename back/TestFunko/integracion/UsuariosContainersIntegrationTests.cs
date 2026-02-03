using System.Threading.Channels;
using FluentAssertions;
using FunkoApi.Service.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace TestFunko.integracion;

/// <summary>
/// Tests de integración para Containers de Usuarios.
/// Verifica la conectividad y configuración de containers Docker (PostgreSQL, MongoDB).
/// </summary>
[TestFixture]
public class UsuariosContainersIntegrationTests
{
    private PostgreSqlContainer? _postgresContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("tienda_test")
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

    [Test]
    public async Task PostgreSQLContainer_ShouldBeRunning()
    {
        _postgresContainer.Should().NotBeNull();
        var connectionString = _postgresContainer!.GetConnectionString();
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain("Host=");

        await Task.CompletedTask;
    }



    [Test]
    public async Task Configuration_CanBuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _postgresContainer!.GetConnectionString() },

                { "MongoDbSettings:DatabaseName", "funko" },
                { "Jwt:Key", "TestKeyWithAtLeast32CharactersForSecurity!" },
                { "Jwt:Issuer", "TiendaApiTest" },
                { "Jwt:Audience", "TiendaApiTest" }
            }!)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());

        using var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();

        await Task.CompletedTask;
    }

    [Test]
    public async Task Configuration_CanGetConnectionStrings()
    {
        var postgresConn = _postgresContainer!.GetConnectionString();

        postgresConn.Should().NotBeNullOrEmpty();

        await Task.CompletedTask;
    }
}