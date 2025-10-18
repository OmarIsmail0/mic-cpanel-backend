using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByUsernameAsync(string username);
    }
}
