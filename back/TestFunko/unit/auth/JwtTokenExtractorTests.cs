using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FunkoApi.Models;
using FunkoApi.Service.auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace TestFunko.unit.auth;

/// <summary>
/// Tests unitarios para JwtTokenExtractor siguiendo principios FIRST.
/// Valida extracción de información de tokens JWT sin validar firma.
/// </summary>
[TestFixture]
public class JwtTokenExtractorTests
{
    private Mock<ILogger<JwtTokenExtractor>> _mockLogger;
    private JwtTokenExtractor _tokenExtractor;
    
    // Configuración para generar tokens de prueba
    private Mock<IConfiguration> _mockConfiguration;
    private JwtService _jwtService;
    private const string ClaveSecretaParaTokensDePrueba = "ClaveSecretaSuperSeguraConMasDe32CaracteresParaHMACSHA256";

    [SetUp]
    public void Setup()
    {
        // Inicializar mocks y servicios para cada test (principio FIRST: Independent)
        _mockLogger = new Mock<ILogger<JwtTokenExtractor>>();
        _tokenExtractor = new JwtTokenExtractor(_mockLogger.Object);
        
        // Configurar JwtService para generar tokens de prueba
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ClaveSecretaParaTokensDePrueba);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        
        var mockJwtServiceLogger = new Mock<ILogger<JwtService>>();
        _jwtService = new JwtService(_mockConfiguration.Object, mockJwtServiceLogger.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Genera un token JWT de prueba con los datos del usuario especificado.
    /// Utiliza JwtService real para crear tokens válidos.
    /// </summary>
    private string GenerarTokenDePrueba(long userId, string username, string email, string role)
    {
        var usuario = new User
        {
            Id = userId,
            Username = username,
            Email = email,
            Role = role,
            PasswordHash = "hash_no_relevante",
            CreatedAt = DateTime.UtcNow
        };
        
        return _jwtService.GenerateToken(usuario);
    }

    /// <summary>
    /// Crea un token JWT manualmente sin usar JwtService.
    /// Útil para testear tokens con estructura específica o sin ciertos claims.
    /// </summary>
    private string CrearTokenManual(Dictionary<string, string> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ClaveSecretaParaTokensDePrueba));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = claims.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();
        claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claimsList,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion

    #region ExtractUserId Tests
    

    [Test]
    public void ExtractUserId_ConTokenSinClaimDeUserId_DebeRetornarNull()
    {
        // Arrange: Crear token sin claim de userId (solo email y role)
        var tokenSinUserId = CrearTokenManual(new Dictionary<string, string>
        {
            { JwtRegisteredClaimNames.Email, "test@example.com" },
            { ClaimTypes.Role, "USER" }
        });

        // Act: Intentar extraer userId de token sin ese claim
        var userIdExtraido = _tokenExtractor.ExtractUserId(tokenSinUserId);

        // Assert: Verificar que retorna null
        Assert.That(userIdExtraido, Is.Null, "Debería retornar null cuando no hay claim de userId");
    }

    [Test]
    public void ExtractUserId_ConTokenMalformado_DebeRetornarNull()
    {
        // Arrange: String que no es un token JWT válido
        var tokenMalformado = "esto.no.es.un.token";

        // Act: Intentar extraer userId de token malformado
        var userIdExtraido = _tokenExtractor.ExtractUserId(tokenMalformado);

        // Assert: Verificar que retorna null y no lanza excepción
        Assert.That(userIdExtraido, Is.Null, "Debería retornar null con token malformado");
    }

    [Test]
    public void ExtractUserId_ConStringVacio_DebeRetornarNull()
    {
        // Arrange: String vacío
        var tokenVacio = "";

        // Act: Intentar extraer userId
        var userIdExtraido = _tokenExtractor.ExtractUserId(tokenVacio);

        // Assert: Verificar que retorna null
        Assert.That(userIdExtraido, Is.Null, "Debería retornar null con string vacío");
    }

    #endregion

    #region ExtractRole Tests

    [Test]
    public void ExtractRole_ConTokenValidoConRole_DebeRetornarRolDelUsuario()
    {
        // Arrange: Generar token con role específico
        var roleEsperado = "ADMIN";
        var tokenConRole = GenerarTokenDePrueba(1, "admin_user", "admin@example.com", roleEsperado);

        // Act: Extraer role del token
        var roleExtraido = _tokenExtractor.ExtractRole(tokenConRole);

        // Assert: Verificar que se extrajo el role correcto
        Assert.That(roleExtraido, Is.Not.Null, "El role no debería ser null");
        Assert.That(roleExtraido, Is.EqualTo(roleEsperado), "El role extraído debería coincidir");
    }

    [Test]
    public void ExtractRole_ConTokenSinClaimDeRole_DebeRetornarNull()
    {
        // Arrange: Crear token sin claim de role
        var tokenSinRole = CrearTokenManual(new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, "123" },
            { JwtRegisteredClaimNames.Email, "test@example.com" }
        });

        // Act: Intentar extraer role de token sin ese claim
        var roleExtraido = _tokenExtractor.ExtractRole(tokenSinRole);

        // Assert: Verificar que retorna null
        Assert.That(roleExtraido, Is.Null, "Debería retornar null cuando no hay claim de role");
    }

    [Test]
    public void ExtractRole_ConTokenMalformado_DebeRetornarNull()
    {
        // Arrange: Token inválido
        var tokenInvalido = "token.invalido.xyz";

        // Act: Intentar extraer role
        var roleExtraido = _tokenExtractor.ExtractRole(tokenInvalido);

        // Assert: Verificar que retorna null sin lanzar excepción
        Assert.That(roleExtraido, Is.Null, "Debería retornar null con token malformado");
    }

    #endregion

    #region IsAdmin Tests

    [Test]
    public void IsAdmin_ConRoleADMIN_DebeRetornarTrue()
    {
        // Arrange: Generar token con role ADMIN
        var tokenAdmin = GenerarTokenDePrueba(1, "admin_user", "admin@example.com", "ADMIN");

        // Act: Verificar si es admin
        var esAdmin = _tokenExtractor.IsAdmin(tokenAdmin);

        // Assert: Verificar que retorna true
        Assert.That(esAdmin, Is.True, "Debería retornar true para role ADMIN");
    }

    [Test]
    public void IsAdmin_ConRoleAdminMinusculas_DebeRetornarTrue()
    {
        // Arrange: Generar token con role "admin" en minúsculas (case-insensitive)
        var tokenAdminMinusculas = GenerarTokenDePrueba(1, "admin_user", "admin@example.com", "admin");

        // Act: Verificar si es admin
        var esAdmin = _tokenExtractor.IsAdmin(tokenAdminMinusculas);

        // Assert: Verificar que retorna true (case-insensitive)
        Assert.That(esAdmin, Is.True, "Debería retornar true para 'admin' en minúsculas");
    }

    [Test]
    public void IsAdmin_ConRoleUSER_DebeRetornarFalse()
    {
        // Arrange: Generar token con role USER
        var tokenUser = GenerarTokenDePrueba(1, "normal_user", "user@example.com", "USER");

        // Act: Verificar si es admin
        var esAdmin = _tokenExtractor.IsAdmin(tokenUser);

        // Assert: Verificar que retorna false
        Assert.That(esAdmin, Is.False, "Debería retornar false para role USER");
    }

    [Test]
    public void IsAdmin_ConTokenSinRole_DebeRetornarFalse()
    {
        // Arrange: Token sin claim de role
        var tokenSinRole = CrearTokenManual(new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, "123" },
            { JwtRegisteredClaimNames.Email, "test@example.com" }
        });

        // Act: Verificar si es admin
        var esAdmin = _tokenExtractor.IsAdmin(tokenSinRole);

        // Assert: Verificar que retorna false
        Assert.That(esAdmin, Is.False, "Debería retornar false cuando no hay role");
    }

    #endregion

    #region ExtractUserInfo Tests



    #endregion

    #region ExtractClaims Tests

    [Test]
    public void ExtractClaims_ConTokenValido_DebeRetornarClaimsPrincipal()
    {
        // Arrange: Generar token válido
        var tokenValido = GenerarTokenDePrueba(77, "usuario_claims", "claims@test.com", "USER");

        // Act: Extraer ClaimsPrincipal
        var claimsPrincipal = _tokenExtractor.ExtractClaims(tokenValido);

        // Assert: Verificar que se obtuvo ClaimsPrincipal válido
        Assert.That(claimsPrincipal, Is.Not.Null, "ClaimsPrincipal no debería ser null");
        Assert.That(claimsPrincipal.Identity, Is.Not.Null, "Identity no debería ser null");
        Assert.That(claimsPrincipal.Claims.Any(), Is.True, "Debería tener al menos un claim");
    }

    [Test]
    public void ExtractClaims_ConTokenValido_DeberiaContenerClaimsEsperados()
    {
        // Arrange: Generar token con datos conocidos
        var tokenValido = GenerarTokenDePrueba(88, "usuario_test", "test@example.com", "ADMIN");

        // Act: Extraer claims
        var claimsPrincipal = _tokenExtractor.ExtractClaims(tokenValido);

        // Assert: Verificar que contiene los claims esperados
        Assert.That(claimsPrincipal, Is.Not.Null);
        
        var claims = claimsPrincipal.Claims.ToList();
        var tieneClaimEmail = claims.Any(c => c.Type == ClaimTypes.Email || c.Type == JwtRegisteredClaimNames.Email);
        var tieneClaimRole = claims.Any(c => c.Type == ClaimTypes.Role);
        
        Assert.That(tieneClaimEmail, Is.True, "Debería tener claim de email");
        Assert.That(tieneClaimRole, Is.True, "Debería tener claim de role");
    }

    [Test]
    public void ExtractClaims_ConTokenMalformado_DebeRetornarNull()
    {
        // Arrange: Token inválido
        var tokenMalformado = "no.es.token";

        // Act: Intentar extraer claims
        var claimsPrincipal = _tokenExtractor.ExtractClaims(tokenMalformado);

        // Assert: Verificar que retorna null
        Assert.That(claimsPrincipal, Is.Null, "Debería retornar null con token malformado");
    }

    #endregion

    #region ExtractEmail Tests

    [Test]
    public void ExtractEmail_ConTokenValidoConEmail_DebeRetornarEmailDelUsuario()
    {
        // Arrange: Generar token con email específico
        var emailEsperado = "usuario@ejemplo.com";
        var tokenConEmail = GenerarTokenDePrueba(1, "usuario", emailEsperado, "USER");

        // Act: Extraer email del token
        var emailExtraido = _tokenExtractor.ExtractEmail(tokenConEmail);

        // Assert: Verificar que se extrajo el email correcto
        Assert.That(emailExtraido, Is.Not.Null, "El email no debería ser null");
        Assert.That(emailExtraido, Is.EqualTo(emailEsperado), "El email extraído debería coincidir");
    }

    [Test]
    public void ExtractEmail_ConTokenSinEmail_DebeRetornarNull()
    {
        // Arrange: Token sin claim de email
        var tokenSinEmail = CrearTokenManual(new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, "123" },
            { ClaimTypes.Role, "USER" }
        });

        // Act: Intentar extraer email
        var emailExtraido = _tokenExtractor.ExtractEmail(tokenSinEmail);

        // Assert: Verificar que retorna null
        Assert.That(emailExtraido, Is.Null, "Debería retornar null cuando no hay email");
    }

    [Test]
    public void ExtractEmail_ConTokenMalformado_DebeRetornarNull()
    {
        // Arrange: Token inválido
        var tokenInvalido = "token.malformado";

        // Act: Intentar extraer email
        var emailExtraido = _tokenExtractor.ExtractEmail(tokenInvalido);

        // Assert: Verificar que retorna null
        Assert.That(emailExtraido, Is.Null, "Debería retornar null con token malformado");
    }

    #endregion

    #region IsValidTokenFormat Tests

    [Test]
    public void IsValidTokenFormat_ConTokenJWTValido_DebeRetornarTrue()
    {
        // Arrange: Generar token JWT válido
        var tokenValido = GenerarTokenDePrueba(1, "usuario", "user@test.com", "USER");

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenValido);

        // Assert: Verificar que retorna true
        Assert.That(esFormatoValido, Is.True, "Un token JWT válido debería tener formato válido");
    }

    [Test]
    public void IsValidTokenFormat_ConTokenDeTresPartesConPuntos_DebeRetornarTrue()
    {
        // Arrange: String con formato JWT básico (3 partes separadas por puntos)
        var tokenFormatoBasico = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenFormatoBasico);

        // Assert: Verificar que retorna true
        Assert.That(esFormatoValido, Is.True, "Debería aceptar formato JWT con 3 partes válidas");
    }

    [Test]
    public void IsValidTokenFormat_ConStringSinPuntos_DebeRetornarFalse()
    {
        // Arrange: String sin puntos (no es formato JWT)
        var tokenSinPuntos = "tokensinpuntosnoesvalido";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenSinPuntos);

        // Assert: Verificar que retorna false
        Assert.That(esFormatoValido, Is.False, "Debería retornar false si no tiene puntos");
    }

    [Test]
    public void IsValidTokenFormat_ConMenosDeTresPartes_DebeRetornarFalse()
    {
        // Arrange: String con solo dos partes
        var tokenIncompleto = "header.payload";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenIncompleto);

        // Assert: Verificar que retorna false
        Assert.That(esFormatoValido, Is.False, "Debería retornar false con menos de 3 partes");
    }

    [Test]
    public void IsValidTokenFormat_ConStringVacio_DebeRetornarFalse()
    {
        // Arrange: String vacío
        var tokenVacio = "";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenVacio);

        // Assert: Verificar que retorna false
        Assert.That(esFormatoValido, Is.False, "Debería retornar false con string vacío");
    }

    [Test]
    public void IsValidTokenFormat_ConWhitespace_DebeRetornarFalse()
    {
        // Arrange: Solo espacios en blanco
        var tokenWhitespace = "   ";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenWhitespace);

        // Assert: Verificar que retorna false
        Assert.That(esFormatoValido, Is.False, "Debería retornar false con whitespace");
    }

    [Test]
    public void IsValidTokenFormat_ConPartesVacias_DebeRetornarFalse()
    {
        // Arrange: Token con partes vacías (puntos pero sin contenido)
        var tokenPartesVacias = "..";

        // Act: Validar formato
        var esFormatoValido = _tokenExtractor.IsValidTokenFormat(tokenPartesVacias);

        // Assert: Verificar que retorna false
        Assert.That(esFormatoValido, Is.False, "Debería retornar false si las partes están vacías");
    }

    #endregion
}
