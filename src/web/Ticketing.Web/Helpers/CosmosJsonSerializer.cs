using System.Text;
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
        if (stream == null)
        {
            return default(T)!;
        }

        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
            return (T)(object)stream;
        }

        // Ensure stream is at the beginning
        if (stream.CanSeek && stream.Position != 0)
        {
            stream.Position = 0;
        }

        // Read the JSON from the stream
        using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
        {
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(json, _options)!;
        }
        // Note: Cosmos DB SDK will dispose the stream after this method returns
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, _options);
        stream.Position = 0;
        return stream;
    }
}

