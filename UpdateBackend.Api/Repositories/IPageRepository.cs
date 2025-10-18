using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public interface IPageRepository : IRepository<Page>
    {
        Task<Page?> GetByPageNameAsync(string pageName);
        Task<Page?> UpdateNestedFieldAsync(string id, string fieldPath, object value);
    }
}
