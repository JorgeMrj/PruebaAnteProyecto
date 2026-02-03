using FunkoApi.Data;
using FunkoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApi.config;

/// <summary>
/// Contexto de base de datos para la aplicación FunkoApi.
/// Define las tablas, relaciones y datos iniciales (seeding).
/// </summary>
public class FunkoDbContext : DbContext
{
    /// <summary>
    /// Configura el modelo de datos, indices y datos iniciales.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SeedData(modelBuilder); 
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            entity.ConfigureTimestamps();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasQueryFilter(u => !u.IsDeleted);
        });
    }
    
    
    public DbSet<Funko> Funkos { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
  /// <summary>
  /// Inicializa una nueva instancia del contexto de base de datos.
  /// </summary>
  /// <param name="options">Opciones de configuración del contexto.</param>
    public FunkoDbContext(DbContextOptions<FunkoDbContext> options)
        : base(options)
    { }
   
        
    /// <summary>
    /// Método privado para sembrar datos de prueba en la base de datos.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos.</param>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Define category IDs
        var categoriaAnimeId = Guid.NewGuid();
        var categoriaPeliculasId = Guid.NewGuid();
        var categoriaVideojuegosId = Guid.NewGuid();

        // Seed categories first
        var categoria1 = new Categoria
        {
            Id = categoriaAnimeId,
            Nombre = "Anime"
        };

        var categoria2 = new Categoria
        {
            Id = categoriaPeliculasId,
            Nombre = "Películas"
        };

        var categoria3 = new Categoria
        {
            Id = categoriaVideojuegosId,
            Nombre = "Videojuegos"
        };

        modelBuilder.Entity<Categoria>().HasData(categoria1, categoria2, categoria3);

        // Seed funkos using CategoryId (foreign key), not Category navigation property
        var funko1 = new Funko
        {
            Id = 1,
            Name = "Goku Super Saiyan",
            CategoryId = categoriaAnimeId,  
            Imagen = "goku.png",
            Price = 19.99,
            CreatedAt = DateTime.UtcNow,  
            UpdatedAt = DateTime.UtcNow 
        };

        var funko2 = new Funko
        {
            Id = 2,
            Name = "Darth Vader",
            CategoryId = categoriaPeliculasId, 
            Imagen = "vader.png",
            Price = 24.50,
            CreatedAt = DateTime.UtcNow,  
            UpdatedAt = DateTime.UtcNow 
        };

        var funko3 = new Funko
        {
            Id = 3,
            Name = "Mario Bros",
            CategoryId = categoriaVideojuegosId,  
            Imagen = Funko.IMAGE_DEFAULT,
            Price = 17.75,
            CreatedAt = DateTime.UtcNow,  
            UpdatedAt = DateTime.UtcNow 
        };

        modelBuilder.Entity<Funko>().HasData(funko1, funko2, funko3);
        // Seed users
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@funkoapi.com",
            PasswordHash = "$2a$12$n1uTaycq1Cq5uwwHCMSqa.dUDZZ3rU4B6.vZPDov4QJiCBgGvCcMy",
            Role = User.UserRoles.ADMIN,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var normalUser = new User
        {
            Id = 2,
            Username = "user",
            Email = "user@funkoapi.com",
            PasswordHash = "$2a$12$Vp5ZpZik9vTjMMLRblbDKu93ct9qZEK/3zMKdOrE7JBdFBBJEogGy",
            Role = User.UserRoles.USER,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<User>().HasData(adminUser, normalUser);

    }
    
}