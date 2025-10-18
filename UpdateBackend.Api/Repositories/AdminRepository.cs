using MongoDB.Driver;
using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public class AdminRepository : MongoRepository<Admin>, IAdminRepository
    {
        public AdminRepository(IMongoDatabase database) : base(database, "admins")
        {
        }

        public async Task<Admin?> GetByUsernameAsync(string username)
        {
            return await GetByFieldAsync(a => a.Username == username);
        }
    }
}
