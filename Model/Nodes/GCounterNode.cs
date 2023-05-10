using System.Text.Json;
using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public class GCounterNode : Node
{
    // stored all delta values seen so far
    private Dictionary<string, long> _deltaStore = new();
    public GCounterNode() : base()
    {
        // shadow the base init
        _handlers[MessageType.INIT] = HandleInit;
        _handlers[MessageType.ADD] = HandleAdd;
        _handlers[MessageType.READ] = HandleRead;
        _handlers[MessageType.GCounterSync] = HandleSync;
    }

    public new Message HandleInit(Message message)
    {
        InitMessageBody initMessageBody = (InitMessageBody)message.Body;
        NodeId = initMessageBody.NodeId;
        NodeIds = initMessageBody.NodeIds;

        foreach (var nodeId in NodeIds)
        {
            _deltaStore[nodeId] = 0;
        }

        // start a background task that sends heartbeat message for detla value every 1 second.
        Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                foreach (var nodeId in NodeIds)
                {
                    if (nodeId != NodeId)
                    {
                        var heartbeat = new Message()
                        {
                            Src = NodeId,
                            Dest = nodeId,
                            Body = new GCounterSyncMessageBody()
                            {
                                Type = MessageType.GCounterSync,
                                Detla = _deltaStore[NodeId],
                            }
                        };
                        _stdout.WriteLine(JsonSerializer.Serialize(heartbeat));
                    }
                }
            }
        });

        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new MessageBody()
            {
                Type = "init_ok",
                InReplyTo = initMessageBody.MsgId,
            },
        };
    }

    public Message HandleAdd(Message message)
    {
        AddMessageBody messageBody = (AddMessageBody)message.Body;
        _deltaStore[NodeId] += messageBody.Detla;
        return new Message
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new MessageBody
            {
                Type = "add_ok",
                InReplyTo = messageBody.MsgId,
            }
        };
    }

    public Message HandleRead(Message message)
    {
        long result = _deltaStore.Values.Sum();
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new GCounterReadMessageBody
            {
                Type = "read_ok",
                InReplyTo = message.Body.MsgId,
                Value = result,
            }
        };
    }

    public Message HandleSync(Message message)
    {
        GCounterSyncMessageBody messageBody = (GCounterSyncMessageBody)message.Body;

        _deltaStore[message.Src] = messageBody.Detla;
        return new Message
        {
            Src = NodeId,
            Dest = NodeId,
        };
    }
}