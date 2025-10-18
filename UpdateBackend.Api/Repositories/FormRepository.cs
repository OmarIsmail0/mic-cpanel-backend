using MongoDB.Driver;
using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public class FormRepository : MongoRepository<Form>, IFormRepository
    {
        public FormRepository(IMongoDatabase database) : base(database, "forms")
        {
        }

        public async Task<Form?> GetByFormNameAsync(string formName)
        {
            return await GetByFieldAsync(f => f.FormName == formName);
        }
    }
}
