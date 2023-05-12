using System.Text.Json.Serialization;

namespace Sharpstone.Model.Messages;

public class TopologyMessageBody : MessageBody
{
    [JsonPropertyName("topology")]
    public Dictionary<string, List<string>> Topology { get; set; } = null!;
}

public class BroadcastMessageBody : MessageBody
{
    [JsonPropertyName("message")]
    public long Message { get; set; }
}

// This is used for broadcast challenge only.
public class BroadcastReadResponseMessageBody : MessageBody
{
    [JsonPropertyName("messages")]
    public List<long>? Messages { get; set; }
}

public class BroadcastSyncMessageBody : MessageBody
{
    // this variable stores the number of message have received from other nodes.
    [JsonPropertyName("known_position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, int>? KnownPositions { get; set; }
}

public class BroadcastSyncResponseMessageBody : BroadcastSyncMessageBody
{
    [JsonPropertyName("messages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, List<long>>? Messages { get; set; }
}
