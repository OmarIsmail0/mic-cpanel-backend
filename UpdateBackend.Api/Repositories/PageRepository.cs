using MongoDB.Bson;
using MongoDB.Driver;
using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public class PageRepository : MongoRepository<Page>, IPageRepository
    {
        public PageRepository(IMongoDatabase database) : base(database, "pages")
        {
        }

        public async Task<Page?> GetByPageNameAsync(string pageName)
        {
            return await GetByFieldAsync(p => p.PageName == pageName);
        }

        public async Task<Page?> UpdateNestedFieldAsync(string id, string fieldPath, object value)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }

            // Get the existing page
            var existingPage = await GetByIdAsync(id);
            if (existingPage == null)
            {
                return null;
            }

            // Convert existing sections to BsonDocument for manipulation
            BsonDocument sectionsDoc;
            if (existingPage.Sections is string sectionsJson)
            {
                sectionsDoc = BsonDocument.Parse(sectionsJson);
            }
            else if (existingPage.Sections is BsonDocument existingBson)
            {
                sectionsDoc = existingBson;
            }
            else
            {
                sectionsDoc = BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(existingPage.Sections));
            }

            // Update the nested field in the BsonDocument
            UpdateNestedFieldInBsonDocument(sectionsDoc, fieldPath, value);

            // Store as JSON string to avoid serialization wrappers
            var jsonString = sectionsDoc.ToJson();

            var filter = Builders<Page>.Filter.Eq("_id", objectId);
            var update = Builders<Page>.Update.Set("sections", jsonString);
            var result = await _collection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? await GetByIdAsync(id) : null;
        }

        private void UpdateNestedFieldInBsonDocument(BsonDocument document, string fieldPath, object value)
        {
            var pathParts = fieldPath.Split('.');
            var current = document;

            // Navigate to the parent of the target field
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var part = pathParts[i];
                var nextPart = i + 1 < pathParts.Length ? pathParts[i + 1] : null;

                // Check if the next part is an array index
                if (nextPart != null && int.TryParse(nextPart, out int arrayIndex))
                {
                    // This is an array field, ensure it's an array
                    if (!current.Contains(part) || !current[part].IsBsonArray)
                    {
                        current[part] = new BsonArray();
                    }

                    var array = current[part].AsBsonArray;

                    // Ensure the array has enough elements
                    while (array.Count <= arrayIndex)
                    {
                        array.Add(BsonValue.Create("")); // Add empty string for missing elements
                    }

                    // If this is the last part (array index), set the value directly
                    if (i == pathParts.Length - 2)
                    {
                        array[arrayIndex] = BsonValue.Create(value);
                        return;
                    }

                    // Skip the array index in the next iteration
                    i++;
                    current = array[arrayIndex].AsBsonDocument ?? new BsonDocument();
                }
                else
                {
                    // Regular field
                    if (!current.Contains(part) || !current[part].IsBsonDocument)
                    {
                        current[part] = new BsonDocument();
                    }
                    current = current[part].AsBsonDocument;
                }
            }

            // Set the final field
            var finalField = pathParts[pathParts.Length - 1];
            current[finalField] = BsonValue.Create(value);
        }
    }
}
