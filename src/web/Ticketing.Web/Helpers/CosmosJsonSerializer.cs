using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

namespace Ticketing.Web.Helpers;

public class CosmosJsonSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;

    public CosmosJsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        // Add JsonStringEnumConverter to handle enum serialization as strings
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json, _options)!;
            }
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, _options);
        stream.Position = 0;
        return stream;
    }
}

