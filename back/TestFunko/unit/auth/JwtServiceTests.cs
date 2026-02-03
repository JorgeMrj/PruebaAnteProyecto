using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FunkoApi.Models;
using FunkoApi.Service.auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestFunko.unit.auth;

/// <summary>
/// Tests unitarios para JwtService siguiendo principios FIRST.
/// Valida generación y validación de tokens JWT usando mocks.
/// </summary>
[TestFixture]
public class JwtServiceTests
{
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<JwtService>> _mockLogger;
    private JwtService _jwtService;

    // Constantes para configuración de JWT usadas en múltiples tests
    private const string ClaveJwtSecretaParaTests = "ClaveSecretaSuperSeguraConMasDe32CaracteresParaHMACSHA256";
    private const string IssuerPorDefecto = "TiendaApi";
    private const string AudiencePorDefecto = "TiendaApi";
    private const string MinutosExpiracionPorDefecto = "60";

    [SetUp]
    public void Setup()
    {
        // Inicializar mocks frescos para cada test (principio FIRST: Independent)
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtService>>();
    }

    #region Helper Methods

    /// <summary>
    /// Configura el mock de IConfiguration con valores estándar para JWT.
    /// Centraliza la configuración para evitar duplicación de código.
    /// </summary>
    private void ConfigurarMockConfiguracionConValoresEstandar()
    {
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveJwtSecretaParaTests);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(IssuerPorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(AudiencePorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns(MinutosExpiracionPorDefecto);
    }

    /// <summary>
    /// Crea un usuario de prueba con datos estándar.
    /// Utilizado en tests que necesitan un usuario válido.
    /// </summary>
    private User CrearUsuarioDePrueba()
    {
        return new User
        {
            Id = 42,
            Username = "usuario_test",
            Email = "test@example.com",
            Role = User.UserRoles.USER,
            PasswordHash = "hash_no_relevante_para_jwt",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Decodifica un token JWT sin validar su firma.
    /// Útil para inspeccionar claims en tests.
    /// </summary>
    private JwtSecurityToken DecodificarTokenSinValidar(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(token);
    }

    #endregion

    #region GenerateToken Tests

    [Test]
    public void GenerateToken_ConConfiguracionValida_DebeCrearTokenConClaimsCorrectos()
    {
        // Arrange: Configurar servicio con valores JWT válidos
        ConfigurarMockConfiguracionConValoresEstandar();
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        
        var usuarioParaToken = CrearUsuarioDePrueba();

        // Act: Generar token JWT para el usuario
        var tokenGenerado = _jwtService.GenerateToken(usuarioParaToken);

        // Assert: Verificar que se generó un token válido
        Assert.That(tokenGenerado, Is.Not.Null, "El token no debería ser null");
        Assert.That(tokenGenerado, Is.Not.Empty, "El token no debería estar vacío");
        
        // Decodificar el token para verificar los claims
        var tokenDecodificado = DecodificarTokenSinValidar(tokenGenerado);
        
        // Verificar claims del usuario
        var claimUsername = tokenDecodificado.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        Assert.That(claimUsername, Is.EqualTo(usuarioParaToken.Username), "El claim Sub debería contener el username");
        
        var claimEmail = tokenDecodificado.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        Assert.That(claimEmail, Is.EqualTo(usuarioParaToken.Email), "El claim Email debería coincidir");
        
        var claimRole = tokenDecodificado.Claims.First(c => c.Type == ClaimTypes.Role).Value;
        Assert.That(claimRole, Is.EqualTo(usuarioParaToken.Role), "El claim Role debería coincidir");
        
        var claimUserId = tokenDecodificado.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        Assert.That(claimUserId, Is.EqualTo(usuarioParaToken.Id.ToString()), "El claim NameIdentifier debería contener el ID");
        
        // Verificar que tiene un JTI (JWT ID único)
        var claimJti = tokenDecodificado.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.That(claimJti, Is.Not.Null, "El token debería tener un claim JTI (JWT ID)");
        
        // Verificar issuer y audience
        Assert.That(tokenDecodificado.Issuer, Is.EqualTo(IssuerPorDefecto), "El issuer debería coincidir");
        Assert.That(tokenDecodificado.Audiences.First(), Is.EqualTo(AudiencePorDefecto), "El audience debería coincidir");
    }

    [Test]
    public void GenerateToken_ConValoresPorDefecto_DebeUsarIssuerYAudiencePredeterminados()
    {
        // Arrange: Configurar solo la clave, sin issuer/audience/expireMinutes
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveJwtSecretaParaTests);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns((string?)null);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns((string?)null);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns((string?)null);
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        var usuarioParaToken = CrearUsuarioDePrueba();

        // Act: Generar token con configuración mínima
        var tokenGenerado = _jwtService.GenerateToken(usuarioParaToken);

        // Assert: Verificar que usa valores por defecto
        var tokenDecodificado = DecodificarTokenSinValidar(tokenGenerado);
        
        Assert.That(tokenDecodificado.Issuer, Is.EqualTo("TiendaApi"), "Debería usar issuer por defecto");
        Assert.That(tokenDecodificado.Audiences.First(), Is.EqualTo("TiendaApi"), "Debería usar audience por defecto");
        
        // Verificar que la expiración es aproximadamente 60 minutos (por defecto)
        var tiempoExpiracion = tokenDecodificado.ValidTo - DateTime.UtcNow;
        Assert.That(tiempoExpiracion.TotalMinutes, Is.GreaterThan(59).And.LessThan(61), 
            "La expiración debería ser aproximadamente 60 minutos por defecto");
    }

    [Test]
    public void GenerateToken_SinClaveJwtConfigurada_DebeLanzarInvalidOperationException()
    {
        // Arrange: Configurar sin Jwt:Key (simular configuración faltante)
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns((string?)null);
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        
        var usuarioParaToken = CrearUsuarioDePrueba();

        // Act & Assert: Verificar que lanza excepción cuando falta la clave
        var excepcion = Assert.Throws<InvalidOperationException>(() => 
            _jwtService.GenerateToken(usuarioParaToken)
        );
        
        Assert.That(excepcion.Message, Does.Contain("JWT Key no configurada"), 
            "El mensaje de error debería indicar que falta la clave JWT");
    }

    [Test]
    public void GenerateToken_ConExpiracionPersonalizada_DebeUsarTiempoConfigurado()
    {
        // Arrange: Configurar con tiempo de expiración de 30 minutos
        var minutosExpiracionPersonalizados = "30";
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveJwtSecretaParaTests);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(IssuerPorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(AudiencePorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns(minutosExpiracionPersonalizados);
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        var usuarioParaToken = CrearUsuarioDePrueba();

        // Act: Generar token con expiración personalizada
        var tokenGenerado = _jwtService.GenerateToken(usuarioParaToken);

        // Assert: Verificar que la expiración es aproximadamente 30 minutos
        var tokenDecodificado = DecodificarTokenSinValidar(tokenGenerado);
        var tiempoExpiracion = tokenDecodificado.ValidTo - DateTime.UtcNow;
        
        Assert.That(tiempoExpiracion.TotalMinutes, Is.GreaterThan(29).And.LessThan(31), 
            "La expiración debería ser aproximadamente 30 minutos según configuración");
    }

    #endregion

    #region ValidateToken Tests

    [Test]
    public void ValidateToken_ConTokenValido_DebeRetornarUsername()
    {
        // Arrange: Generar un token válido primero
        ConfigurarMockConfiguracionConValoresEstandar();
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        
        var usuarioOriginal = CrearUsuarioDePrueba();
        var tokenValido = _jwtService.GenerateToken(usuarioOriginal);

        // Act: Validar el token recién generado
        var usernameExtraido = _jwtService.ValidateToken(tokenValido);

        // Assert: Verificar que retorna el username correcto
        Assert.That(usernameExtraido, Is.Not.Null, "La validación debería retornar un username");
        Assert.That(usernameExtraido, Is.EqualTo(usuarioOriginal.Username), 
            "El username extraído debería coincidir con el original");
    }

    [Test]
    public void ValidateToken_ConTokenExpirado_DebeRetornarNull()
    {
        // Arrange: Configurar con expiración de -1 minuto (ya expirado)
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveJwtSecretaParaTests);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(IssuerPorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(AudiencePorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns("-1");
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        var usuarioParaToken = CrearUsuarioDePrueba();
        
        // Generar token que expira inmediatamente
        var tokenExpirado = _jwtService.GenerateToken(usuarioParaToken);
        
        // Esperar un momento para asegurar que expire
        Thread.Sleep(100);

        // Act: Intentar validar token expirado
        var resultado = _jwtService.ValidateToken(tokenExpirado);

        // Assert: Verificar que retorna null
        Assert.That(resultado, Is.Null, "Un token expirado debería retornar null");
    }

    [Test]
    public void ValidateToken_ConTokenFirmadoConOtraClave_DebeRetornarNull()
    {
        // Arrange: Generar token con una clave
        var claveOriginal = "ClaveOriginal123456789012345678901234567890";
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(claveOriginal);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(IssuerPorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(AudiencePorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        var usuarioParaToken = CrearUsuarioDePrueba();
        var tokenConClaveOriginal = _jwtService.GenerateToken(usuarioParaToken);
        
        // Cambiar la clave en el mock (simular que la clave cambió en el servidor)
        var claveDiferente = "ClaveCompletamenteDiferente1234567890123456789";
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(claveDiferente);
        
        // Crear nuevo servicio con la clave diferente
        var jwtServiceConOtraClave = new JwtService(_mockConfiguration.Object, _mockLogger.Object);

        // Act: Intentar validar token con servicio que usa otra clave
        var resultado = jwtServiceConOtraClave.ValidateToken(tokenConClaveOriginal);

        // Assert: Verificar que retorna null (firma no válida)
        Assert.That(resultado, Is.Null, "Un token firmado con otra clave debería retornar null");
    }

    [Test]
    public void ValidateToken_ConIssuerIncorrecto_DebeRetornarNull()
    {
        // Arrange: Generar token con un issuer
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveJwtSecretaParaTests);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("IssuerOriginal");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(AudiencePorDefecto);
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        var usuarioParaToken = CrearUsuarioDePrueba();
        var tokenConIssuerOriginal = _jwtService.GenerateToken(usuarioParaToken);
        
        // Cambiar el issuer esperado
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("IssuerDiferente");
        var jwtServiceConOtroIssuer = new JwtService(_mockConfiguration.Object, _mockLogger.Object);

        // Act: Intentar validar con issuer diferente
        var resultado = jwtServiceConOtroIssuer.ValidateToken(tokenConIssuerOriginal);

        // Assert: Verificar que retorna null
        Assert.That(resultado, Is.Null, "Un token con issuer incorrecto debería retornar null");
    }

    [Test]
    public void ValidateToken_ConTokenMalformado_DebeRetornarNull()
    {
        // Arrange: Configurar servicio
        ConfigurarMockConfiguracionConValoresEstandar();
        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        
        // Crear strings que NO son tokens JWT válidos
        var tokensMalformados = new[]
        {
            "esto.no.es.un.token.valido",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.token_incompleto",
            "",
            "token_sin_puntos",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0In0.firma_invalida_xyz123"
        };

        // Act & Assert: Verificar que todos retornan null
        foreach (var tokenMalformado in tokensMalformados)
        {
            var resultado = _jwtService.ValidateToken(tokenMalformado);
            Assert.That(resultado, Is.Null, 
                $"Un token malformado '{tokenMalformado.Substring(0, Math.Min(20, tokenMalformado.Length))}...' debería retornar null");
        }
    }

    #endregion
}
