using System.Text.Json.Serialization;

namespace Sharpstone.Model.Messages;

public class AddMessageBody : MessageBody
{
    [JsonPropertyName("delta")]
    public long Detla { get; set; }
}

public class GCounterReadMessageBody : MessageBody
{
    [JsonPropertyName("value")]
    public long Value { get; set; }
}

public class GCounterSyncMessageBody : AddMessageBody { }