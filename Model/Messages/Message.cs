using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpstone.Model.Messages;

public record MessageType
{
    public const string INIT = "init";
    public const string ECHO = "echo";
    public const string GENERATE = "generate";
    public const string TOPOLOGY = "topology";
    public const string BROADCAST = "broadcast";
    public const string BROADCASTSYNC = "broadcast_sync";
    public const string OKBROADCASTSYNC = "ok_gcounter_sync";
    public const string READ = "read";
    public const string ADD = "add";
    public const string GCOUNTERSYNC = "gcounter_sync";
}

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

// This is put here because there are 2 challenges both uses "read" type, with no arguments
public class ReadMessageBody : MessageBody { }

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
            // init
            MessageType.INIT => root.Deserialize<InitMessageBody>(options),
            // echo
            MessageType.ECHO => root.Deserialize<EchoMessageBody>(options),
            // generate unique id
            MessageType.GENERATE => root.Deserialize<UniqueIdMessageBody>(options),
            // broadcast
            MessageType.TOPOLOGY => root.Deserialize<TopologyMessageBody>(options),
            MessageType.BROADCAST => root.Deserialize<BroadcastMessageBody>(options),
            MessageType.BROADCASTSYNC => root.Deserialize<BroadcastSyncMessageBody>(options),
            MessageType.OKBROADCASTSYNC => root.Deserialize<BroadcastSyncResponseMessageBody>(options),
            // grow-only counter
            MessageType.ADD => root.Deserialize<AddMessageBody>(options),
            MessageType.GCOUNTERSYNC => root.Deserialize<GCounterSyncMessageBody>(options), // a custom workload for syncing counter
            // broadcast and grow-only counter
            MessageType.READ => root.Deserialize<ReadMessageBody>(options),
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