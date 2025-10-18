using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public interface IFormRepository : IRepository<Form>
    {
        Task<Form?> GetByFormNameAsync(string formName);
    }
}
