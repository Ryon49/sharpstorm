using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpstone.Model;

public class Message
{
    [JsonPropertyName("src")]
    public string Src { get; set; } = String.Empty;

    [JsonPropertyName("dest")]
    public string Dest { get; set; } = String.Empty;

    [JsonPropertyName("body")]
    [JsonConverter(typeof(PolymorphicMessageBodyConverter))]
    public MessageBody Body { get; set; } = null!;
}

public class MessageBody
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = String.Empty;

    [JsonPropertyName("msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long MsgId { get; set; }

    [JsonPropertyName("in_reply_to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long InReplyTo { get; set; }
}

public class InitMessageBody : MessageBody
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = null!;

    [JsonPropertyName("node_ids")]
    public List<string> NodeIds { get; set; } = null!;
}

public class EchoMessageBody : MessageBody
{
    [JsonPropertyName("echo")]
    public string Echo { get; set; } = String.Empty;
}

public class UniqueIdMessageBody : MessageBody
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Because [JsonPolymorphic] attribute does not support TypeDiscriminatorPropertyName that is not in the first
/// of a json object, PolymorphicMessageBodyConverter manually decides which type of MessageBody to parse.
/// </summary>
sealed public class PolymorphicMessageBodyConverter : JsonConverter<MessageBody>
{
    /// <summary>
    /// Parse message body based on the type
    /// </summary>
    public override MessageBody? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;
        JsonElement type = root.GetProperty("type");

        return type.GetString() switch
        {
            "init" => root.Deserialize<InitMessageBody>(options),
            "echo" => root.Deserialize<EchoMessageBody>(options),
            "generate" => root.Deserialize<UniqueIdMessageBody>(options),
            _ => root.Deserialize<MessageBody>(options),
        };
    }

    /// <summary>
    /// Use default Json serialization with MessageBody's actual type
    /// </summary>
    public override void Write(Utf8JsonWriter writer, MessageBody value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}