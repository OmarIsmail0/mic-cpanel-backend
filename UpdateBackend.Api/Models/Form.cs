using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UpdateBackend.Api.Models
{
    public class Form
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("formName")]
        public string FormName { get; set; } = string.Empty;

        [BsonElement("formData")]
        [JsonConverter(typeof(BsonDocumentJsonConverter))]
        public BsonDocument FormData { get; set; } = new BsonDocument();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BsonDocumentJsonConverter : JsonConverter<BsonDocument>
    {
        public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return BsonDocument.Parse(document.RootElement.GetRawText());
        }

        public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options)
        {
            var json = value.ToJson();
            using var document = JsonDocument.Parse(json);
            document.RootElement.WriteTo(writer);
        }
    }
}
