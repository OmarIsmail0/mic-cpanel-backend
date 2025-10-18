using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UpdateBackend.Api.Models
{
    public class Page
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("page")]
        public string PageName { get; set; } = string.Empty;

        [BsonElement("sections")]
        [JsonConverter(typeof(JsonStringConverter))]
        public object Sections { get; set; } = "{}";

        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();

        [BsonElement("videos")]
        public List<string> Videos { get; set; } = new List<string>();

        [BsonElement("pdfs")]
        public List<string> Pdfs { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class JsonStringConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? "{}";
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var document = JsonDocument.ParseValue(ref reader);
                return document.RootElement.GetRawText();
            }
            return "{}";
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is string str)
            {
                if (string.IsNullOrEmpty(str) || str == "{}")
                {
                    writer.WriteStringValue("{}");
                }
                else
                {
                    // Parse the JSON string and write it as a JSON object
                    try
                    {
                        using var document = JsonDocument.Parse(str);
                        document.RootElement.WriteTo(writer);
                    }
                    catch
                    {
                        writer.WriteStringValue(str);
                    }
                }
            }
            else if (value is BsonDocument bsonDoc)
            {
                // Convert BsonDocument to JSON and write as object
                var json = bsonDoc.ToJson();
                try
                {
                    using var document = JsonDocument.Parse(json);
                    document.RootElement.WriteTo(writer);
                }
                catch
                {
                    writer.WriteStringValue(json);
                }
            }
            else
            {
                // Handle any other object type by serializing it
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(value);
                    using var document = JsonDocument.Parse(json);
                    document.RootElement.WriteTo(writer);
                }
                catch
                {
                    writer.WriteStringValue("{}");
                }
            }
        }
    }
}
