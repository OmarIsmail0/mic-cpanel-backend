using System.Linq.Expressions;

namespace UpdateBackend.Api.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetByFieldAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter);
        Task<T> CreateAsync(T entity);
        Task<T?> UpdateAsync(string id, T entity);
        Task<T?> UpdateFieldAsync(string id, string fieldName, object value);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountAsync(Expression<Func<T, bool>> filter);
    }
}
