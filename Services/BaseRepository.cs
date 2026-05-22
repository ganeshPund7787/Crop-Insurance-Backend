using Authentication.Data;
using Authentication.Interfaces;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Authentication.Services;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ─── Read Operations ───────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    // ─── Write Operations ──────────────────────────────────────────────────

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        // ✅ Only attach if not already tracked
        var tracked = _context.ChangeTracker
            .Entries<T>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (tracked is null)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
        // If already tracked, EF will detect changes automatically
        // No need to set state manually
    }

    // ─── Soft Delete ───────────────────────────────────────────────────────
    // Never permanently delete — mark IsDeleted = true
    // Global query filter in DbContext hides these automatically
    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        Update(entity);
    }

    // ─── Hard Delete ───────────────────────────────────────────────────────
    // Only for admin-level cleanup or GDPR requests
    public void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}