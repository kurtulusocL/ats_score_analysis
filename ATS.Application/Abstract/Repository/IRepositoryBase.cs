using System.Linq.Expressions;
using ATS.Domain.Entities.Base;

namespace ATS.Application.Abstract.Repository
{
    public interface IRepositoryBase<T> where T : class, IEntity, new()
    {
        Task<T?> GetWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> SaveChangesAsync();
    }
}
