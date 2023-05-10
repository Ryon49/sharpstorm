using System.Text.Json.Serialization;

namespace Sharpstone.Model.Messages;

public class UniqueIdMessageBody : MessageBody
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
