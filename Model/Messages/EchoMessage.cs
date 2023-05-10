using System.Text.Json.Serialization;

namespace Sharpstone.Model.Messages;

public class EchoMessageBody : MessageBody
{
    [JsonPropertyName("echo")]
    public string Echo { get; set; } = String.Empty;
}