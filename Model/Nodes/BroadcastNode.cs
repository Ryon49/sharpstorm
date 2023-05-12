using System.Linq;
using System.Text.Json;
using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public class BroadcastNode : Node
{

    private Dictionary<string, List<long>> _messagesStored = new();

    private List<string> _neighbors = new();

    // obvious we need a mutex. but ...

    public BroadcastNode() : base()
    {
        _handlers[MessageType.TOPOLOGY] = HandleTopology;
        _handlers[MessageType.BROADCAST] = HandleBroadcast;
        _handlers[MessageType.READ] = HandleRead;
        _handlers[MessageType.BROADCASTSYNC] = HandleBroadcastSync;
        _handlers[MessageType.OKBROADCASTSYNC] = HandleOkBroadcastSync;
    }

    public Message HandleTopology(Message message)
    {
        TopologyMessageBody messageBody = (TopologyMessageBody)message.Body;
        _neighbors = messageBody.Topology![NodeId];

        // init the message store
        foreach (var nodeId in NodeIds)
        {
            _messagesStored[nodeId] = new();
        }

        Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                Dictionary<string, int> knownPositions = new();
                foreach (var nodeId in NodeIds)
                {
                    // except itself
                    if (nodeId != NodeId)
                    {
                        knownPositions[nodeId] = _messagesStored[nodeId].Count;
                    }
                }

                foreach (var nodeId in _neighbors)
                {
                    var heartbeat = new Message()
                    {
                        Src = NodeId,
                        Dest = nodeId,
                        Body = new BroadcastSyncMessageBody()
                        {
                            Type = MessageType.BROADCASTSYNC,
                            KnownPositions = knownPositions
                        }
                    };
                    _stdout.WriteLine(JsonSerializer.Serialize(heartbeat));
                }
                _stdout.Flush();
            }
        });

        // set the neighbors
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
        this._messagesStored[NodeId].Add(messageBody.Message);
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
        var combined = new List<long>();
        foreach (var messages in _messagesStored.Values)
        {
            combined.AddRange(messages);
        }
        // just making result look nicer
        combined.Sort();
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new BroadcastReadResponseMessageBody
            {
                Type = "read_ok",
                InReplyTo = message.Body.MsgId,
                Messages = combined,
            }
        };
    }

    public Message HandleBroadcastSync(Message message)
    {
        BroadcastSyncMessageBody messageBody = (BroadcastSyncMessageBody)message.Body;
        Dictionary<string, List<long>> messages = new();
        foreach (var entry in messageBody.KnownPositions!)
        {
            var nodeId = entry.Key;
            var position = entry.Value;

            if (_messagesStored[nodeId].Count > position)
            {
                var newList = new List<long>(_messagesStored[nodeId].Skip(position));
                messages[nodeId] = newList;
            }
        }

        if (messages.Count == 0)
        {
            return new Message()
            {
                Src = NodeId,
                Dest = NodeId,
            };
        }
        return new Message
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new BroadcastSyncResponseMessageBody
            {
                Type = MessageType.OKBROADCASTSYNC,
                KnownPositions = messageBody.KnownPositions,
                Messages = messages,
            }
        };
    }

    // it is possible that due to network partition, multiple sync response can arrive together late.
    // So we want to know where to add the entry. 
    public Message HandleOkBroadcastSync(Message message)
    {
        BroadcastSyncResponseMessageBody messageBody = (BroadcastSyncResponseMessageBody)message.Body;

        foreach (var entry in messageBody.Messages!)
        {
            var nodeId = entry.Key;
            var messages = entry.Value;

            var beginIndex = messageBody.KnownPositions![nodeId];
            var newList = new List<long>(_messagesStored[nodeId].Take(beginIndex));
            newList!.AddRange(messages);

            _messagesStored[nodeId] = newList;
        }

        return new Message()
        {
            Src = NodeId,
            Dest = NodeId,
        };
    }
}