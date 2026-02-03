using FunkoApi.Dto.Users;
using FunkoApi.Error;
using FunkoApi.Models;
using FunkoApi.Repository.Users;
using FunkoApi.Service.auth;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestFunko.unit.auth;

/// <summary>
/// Tests unitarios para AuthService siguiendo principios FIRST.
/// Utiliza mocks para aislar la lógica del servicio de autenticación.
/// </summary>
[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IJwtService> _mockJwtService;
    private Mock<ILogger<AuthService>> _mockLogger;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        // Inicializar mocks frescos para cada test (principio FIRST: Independent)
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        
        // Crear instancia del servicio con las dependencias mockeadas
        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockLogger.Object
        );
    }

    #region SignUpAsync Tests

    [Test]
    public async Task SignUpAsync_ConDatosValidos_DebeRegistrarUsuarioYRetornarToken()
    {
        // Arrange: Preparar datos de entrada válidos para un nuevo usuario
        var registroDto = new RegisterDto
        {
            Username = "nuevoUsuario",
            Email = "nuevo@example.com",
            Password = "Password123!"
        };

        // Simular que no existen duplicados en la base de datos
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(registroDto.Username))
            .ReturnsAsync((User?)null);
        
        _mockUserRepository
            .Setup(repo => repo.FindByEmailAsync(registroDto.Email))
            .ReturnsAsync((User?)null);

        // Simular que el usuario se guarda correctamente
        var usuarioGuardado = new User
        {
            Id = 1,
            Username = registroDto.Username,
            Email = registroDto.Email,
            PasswordHash = "hashedPassword",
            Role = User.UserRoles.USER,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockUserRepository
            .Setup(repo => repo.SaveAsync(It.IsAny<User>()))
            .ReturnsAsync(usuarioGuardado);

        // Simular generación de token JWT
        var tokenEsperado = "jwt.token.generado";
        _mockJwtService
            .Setup(jwt => jwt.GenerateToken(It.IsAny<User>()))
            .Returns(tokenEsperado);

        // Act: Ejecutar el método de registro
        var resultado = await _authService.SignUpAsync(registroDto);

        // Assert: Verificar que el resultado sea exitoso
        Assert.That(resultado.IsSuccess, Is.True, "El registro debería ser exitoso");
        Assert.That(resultado.Value.Token, Is.EqualTo(tokenEsperado), "El token debería coincidir");
        Assert.That(resultado.Value.User.Username, Is.EqualTo(registroDto.Username), "El username debería coincidir");
        Assert.That(resultado.Value.User.Email, Is.EqualTo(registroDto.Email), "El email debería coincidir");
        Assert.That(resultado.Value.User.Role, Is.EqualTo(User.UserRoles.USER), "El rol debería ser USER");
        
        // Verificar que se llamó a SaveAsync exactamente una vez
        _mockUserRepository.Verify(repo => repo.SaveAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task SignUpAsync_ConUsernameDuplicado_DebeRetornarConflictError()
    {
        // Arrange: Preparar datos con un username que ya existe
        var registroDto = new RegisterDto
        {
            Username = "usuarioExistente",
            Email = "nuevo@example.com",
            Password = "Password123!"
        };

        // Simular que el username ya existe en la base de datos
        var usuarioExistenteConMismoUsername = new User
        {
            Id = 99,
            Username = registroDto.Username,
            Email = "otro@example.com",
            PasswordHash = "hash",
            Role = User.UserRoles.USER
        };
        
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(registroDto.Username))
            .ReturnsAsync(usuarioExistenteConMismoUsername);
        
        _mockUserRepository
            .Setup(repo => repo.FindByEmailAsync(registroDto.Email))
            .ReturnsAsync((User?)null);

        // Act: Intentar registrar con username duplicado
        var resultado = await _authService.SignUpAsync(registroDto);

        // Assert: Verificar que retorna un error de conflicto
        Assert.That(resultado.IsFailure, Is.True, "El registro debería fallar");
        Assert.That(resultado.Error, Is.InstanceOf<ConflictError>(), "El error debería ser ConflictError");
        Assert.That(resultado.Error.Error, Does.Contain("username"), "El mensaje debería mencionar username");
        
        // Verificar que NO se intentó guardar el usuario
        _mockUserRepository.Verify(repo => repo.SaveAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task SignUpAsync_ConEmailDuplicado_DebeRetornarConflictError()
    {
        // Arrange: Preparar datos con un email que ya existe
        var registroDto = new RegisterDto
        {
            Username = "nuevoUsuario",
            Email = "existente@example.com",
            Password = "Password123!"
        };

        // Simular que el email ya existe en la base de datos
        var usuarioExistenteConMismoEmail = new User
        {
            Id = 88,
            Username = "otroUsuario",
            Email = registroDto.Email,
            PasswordHash = "hash",
            Role = User.UserRoles.USER
        };
        
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(registroDto.Username))
            .ReturnsAsync((User?)null);
        
        _mockUserRepository
            .Setup(repo => repo.FindByEmailAsync(registroDto.Email))
            .ReturnsAsync(usuarioExistenteConMismoEmail);

        // Act: Intentar registrar con email duplicado
        var resultado = await _authService.SignUpAsync(registroDto);

        // Assert: Verificar que retorna un error de conflicto
        Assert.That(resultado.IsFailure, Is.True, "El registro debería fallar");
        Assert.That(resultado.Error, Is.InstanceOf<ConflictError>(), "El error debería ser ConflictError");
        Assert.That(resultado.Error.Error, Does.Contain("email"), "El mensaje debería mencionar email");
        
        // Verificar que NO se intentó guardar el usuario
        _mockUserRepository.Verify(repo => repo.SaveAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region SignInAsync Tests

    [Test]
    public async Task SignInAsync_ConCredencialesValidas_DebeRetornarTokenDeAutenticacion()
    {
        // Arrange: Preparar credenciales válidas
        var loginDto = new LoginDto
        {
            Username = "usuarioValido",
            Password = "Password123!"
        };

        // Crear hash BCrypt real de la contraseña para una validación realista
        var passwordHashReal = BCrypt.Net.BCrypt.HashPassword(loginDto.Password, workFactor: 11);
        
        var usuarioEnBaseDeDatos = new User
        {
            Id = 1,
            Username = loginDto.Username,
            Email = "usuario@example.com",
            PasswordHash = passwordHashReal,
            Role = User.UserRoles.USER,
            CreatedAt = DateTime.UtcNow
        };

        // Simular que el usuario existe en la base de datos
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(loginDto.Username))
            .ReturnsAsync(usuarioEnBaseDeDatos);

        // Simular generación de token JWT
        var tokenEsperado = "jwt.token.autenticado";
        _mockJwtService
            .Setup(jwt => jwt.GenerateToken(It.IsAny<User>()))
            .Returns(tokenEsperado);

        // Act: Ejecutar el login
        var resultado = await _authService.SignInAsync(loginDto);

        // Assert: Verificar que el login fue exitoso
        Assert.That(resultado.IsSuccess, Is.True, "El login debería ser exitoso");
        Assert.That(resultado.Value.Token, Is.EqualTo(tokenEsperado), "El token debería coincidir");
        Assert.That(resultado.Value.User.Username, Is.EqualTo(loginDto.Username), "El username debería coincidir");
        Assert.That(resultado.Value.User.Id, Is.EqualTo(usuarioEnBaseDeDatos.Id), "El ID debería coincidir");
        
        // Verificar que se generó el token exactamente una vez
        _mockJwtService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task SignInAsync_ConUsuarioNoExistente_DebeRetornarUnauthorizedError()
    {
        // Arrange: Preparar credenciales de un usuario que no existe
        var loginDto = new LoginDto
        {
            Username = "usuarioInexistente",
            Password = "Password123!"
        };

        // Simular que el usuario NO existe en la base de datos
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(loginDto.Username))
            .ReturnsAsync((User?)null);

        // Act: Intentar hacer login con usuario inexistente
        var resultado = await _authService.SignInAsync(loginDto);

        // Assert: Verificar que retorna error de no autorizado
        Assert.That(resultado.IsFailure, Is.True, "El login debería fallar");
        Assert.That(resultado.Error, Is.InstanceOf<UnauthorizedError>(), "El error debería ser UnauthorizedError");
        Assert.That(resultado.Error.Error, Does.Contain("credenciales invalidas"), "El mensaje debería indicar credenciales inválidas");
        
        // Verificar que NO se generó ningún token
        _mockJwtService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task SignInAsync_ConPasswordIncorrecta_DebeRetornarUnauthorizedError()
    {
        // Arrange: Preparar usuario válido pero con contraseña incorrecta
        var loginDto = new LoginDto
        {
            Username = "usuarioValido",
            Password = "PasswordIncorrecta!"
        };

        // Crear hash de una contraseña DIFERENTE a la que se intenta usar
        var passwordHashDeLaPasswordCorrecta = BCrypt.Net.BCrypt.HashPassword("PasswordCorrecta123!", workFactor: 11);
        
        var usuarioEnBaseDeDatos = new User
        {
            Id = 1,
            Username = loginDto.Username,
            Email = "usuario@example.com",
            PasswordHash = passwordHashDeLaPasswordCorrecta,
            Role = User.UserRoles.USER
        };

        // Simular que el usuario existe pero la contraseña no coincidirá
        _mockUserRepository
            .Setup(repo => repo.FindByUsernameAsync(loginDto.Username))
            .ReturnsAsync(usuarioEnBaseDeDatos);

        // Act: Intentar login con contraseña incorrecta
        var resultado = await _authService.SignInAsync(loginDto);

        // Assert: Verificar que retorna error de no autorizado
        Assert.That(resultado.IsFailure, Is.True, "El login debería fallar");
        Assert.That(resultado.Error, Is.InstanceOf<UnauthorizedError>(), "El error debería ser UnauthorizedError");
        Assert.That(resultado.Error.Error, Does.Contain("credenciales invalidas"), "El mensaje debería indicar credenciales inválidas");
        
        // Verificar que NO se generó ningún token
        _mockJwtService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #endregion
}
