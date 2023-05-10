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