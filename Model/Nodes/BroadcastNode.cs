using System.Text.Json.Serialization;
using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public class BroadcastNode : Node
{

    private List<long> _messagesStored = new();

    private List<string> _neighbors = new();

    public BroadcastNode() : base()
    {
        _handlers[MessageType.TOPOLOGY] = HandleTopology;
        _handlers[MessageType.BROADCAST] = HandleBroadcast;
        _handlers[MessageType.READ] = HandleRead;
    }

    public Message HandleTopology(Message message)
    {
        TopologyMessageBody messageBody = (TopologyMessageBody)message.Body;

        // set the neighbors
        _neighbors = messageBody.Topology![NodeId];
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new MessageBody
            {
                Type = "topology_ok",
                InReplyTo = messageBody.MsgId,
            }
        };
    }

    public Message HandleBroadcast(Message message)
    {
        BroadcastMessageBody messageBody = (BroadcastMessageBody)message.Body;
        this._messagesStored.Add(messageBody.Message);
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new MessageBody
            {
                Type = "broadcast_ok",
                InReplyTo = messageBody.MsgId,
            }
        };
    }

    public Message HandleRead(Message message)
    {
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new BroadcastReadResponseMessageBody
            {
                Type = "read_ok",
                InReplyTo = message.Body.MsgId,
                Messages = this._messagesStored,
            }
        };
    }
}

// This is used for broadcast challenge only.
public class BroadcastReadResponseMessageBody : MessageBody
{
    [JsonPropertyName("messages")]
    public List<long>? Messages { get; set; }
}