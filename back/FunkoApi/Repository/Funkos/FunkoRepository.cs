using FunkoApi.config;
using FunkoApi.Models;
using Microsoft.EntityFrameworkCore;


namespace FunkoApi.Repository.funkos;

/// <summary>
/// Implementación del repositorio de Funkos utilizando Entity Framework Core.
/// </summary>
public class FunkoRepository(FunkoDbContext context,ILogger<FunkoRepository> log) : IFunkoRepository
{
   
    
    /// <inheritdoc />
    public async Task<List<Funko>> GetAllAsync()
    {
        log.LogInformation("Getting all Funkos");
        return await  context.Funkos
            .Include(f => f.Category)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Funko?> GetByIdAsync(long id)
    {
        log.LogInformation("Getting Funko with id: " + id);
        return await context.Funkos
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    
    /// <inheritdoc />
    public IQueryable<Funko> FindAllAsNoTracking()
    {
        log.LogDebug("Obteniendo productos como IQueryable");
        return context.Funkos
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .AsNoTracking();
    }

    /// <inheritdoc />
    public async Task<Funko?> UpdateAsync(long id, Funko newFunko)
    {
        log.LogInformation("Updating Funko with id: " + id);
        newFunko.Id = id;
        var found=await context.Funkos.FindAsync(id);
        if (found != null)
        {
            found.Name = newFunko.Name;
            found.Category = newFunko.Category;
            found.Price= newFunko.Price;
            found.UpdatedAt= DateTime.UtcNow;
            if (newFunko.Imagen != Funko.IMAGE_DEFAULT)
            {
                found.Imagen = newFunko.Imagen;
            }
            var updated =  context.Funkos.Update(found);
            await context.SaveChangesAsync();
            await context.Funkos.Entry(found).Reference(f => f.Category).LoadAsync();
            return updated.Entity;
        }
        return null;
    }

    /// <inheritdoc />
    public async Task<Funko> AddAsync(Funko newFunko)
    { 
        log.LogInformation("Adding Funko");
        var saved=await context.Funkos.AddAsync(newFunko);
        await context.SaveChangesAsync();
        await context.Funkos.Entry(newFunko).Reference(f => f.Category).LoadAsync();
        return saved.Entity;
    }

    /// <inheritdoc />
    public async Task<Funko?> DeleteAsync(long id)
    {
        log.LogInformation("Deleting Funko with id: " + id);
        var deleted=await context. Funkos
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id.Equals(id)) is { } funko
            ? context.Funkos.Remove(funko).Entity
            : null;
        await context.SaveChangesAsync();
        return deleted;
    }

   
}