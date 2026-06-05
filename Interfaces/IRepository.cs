using Authentication.Models;
using System.Linq.Expressions;

namespace Authentication.Interfaces;

// ─── Generic base repository ───────────────────────────────────────────────
// Every entity gets these operations for free
// Domain-specific repos extend this with custom queries
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);        // Soft delete
    void HardDelete(T entity);    // Permanent delete (admin only)
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
}