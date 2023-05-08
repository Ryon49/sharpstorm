// #nullable disable
using System.Text.Json;

namespace Sharpstone.Model;

public class Node
{
    // set-able, maybe.
    private StreamReader _stdin = new StreamReader(Console.OpenStandardInput());
    private StreamWriter _stdout = new StreamWriter(Console.OpenStandardOutput());
    private StreamWriter _stderr = new StreamWriter(Console.OpenStandardError());

    public string NodeId = String.Empty;
    // store list of node ids that can communicate, regardless of topology
    public List<string> NodeIds = null!;
    // store list of node ids that is to the neighbor in the topology
    public List<string> TopologyNeighbors = null!;

    private Dictionary<string, Func<Message, Message>> _handlers;

    // for broadcast challenge, store all message seen
    private List<long> _messagesStored = new();

    // for Grow-only Counter challenge
    private Dictionary<string, long> _deltaStore = new();
    private CancellationTokenSource _deltaHeartbeat = new();

    public Node()
    {
        _handlers = new Dictionary<string, Func<Message, Message>>()
        {
            {nameof(InitMessageBody), HandleInit},
            {nameof(EchoMessageBody), HandleEcho},
            {nameof(UniqueIdMessageBody), HandleGenerateUniqueId},
            {nameof(TopologyMessageBody), HandleTopology},
            {nameof(BroadcastMessageBody), HandleBroadcast},
            {nameof(ReadMessageBody), HandleRead},
            {nameof(AddMessageBody), HandleAdd},
            {nameof(SyncDeltaMessageBody), HandleSyncDelta},
        };
    }

    public void Run()
    {
        while (true)
        {
            var inputString = _stdin.ReadLine();
            if (String.IsNullOrEmpty(inputString))
            {
                break;
            }

            Message request = JsonSerializer.Deserialize<Message>(inputString)!;
            Message response = _handlers[request!.Body.GetType().Name](request);

            // Some message (like heartbeat) does not require a response 
            if (response.Src != response.Dest)
            {
                _stdout.WriteLine(JsonSerializer.Serialize(response));
                _stdout.Flush();
            }
        }
        _deltaHeartbeat.Cancel();
    }

    public Message HandleInit(Message message)
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
                            Body = new SyncDeltaMessageBody()
                            {
                                Type = "syncDelta",
                                Detla = _deltaStore[NodeId],
                            }
                        };
                        _stdout.WriteLine(JsonSerializer.Serialize(heartbeat));
                    }
                }
            }
        }, _deltaHeartbeat.Token);

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

    public Message HandleEcho(Message message)
    {
        EchoMessageBody echoMessageBody = (EchoMessageBody)message.Body;
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new EchoMessageBody
            {
                Type = "echo_ok",
                MsgId = echoMessageBody.MsgId,
                InReplyTo = echoMessageBody.MsgId,
                Echo = echoMessageBody.Echo,
            }
        };
    }

    public Message HandleGenerateUniqueId(Message message)
    {
        UniqueIdMessageBody messageBody = (UniqueIdMessageBody)message.Body;
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new UniqueIdMessageBody
            {
                Type = "generate_ok",
                InReplyTo = messageBody.MsgId,
            }
        };
    }

    public Message HandleTopology(Message message)
    {
        TopologyMessageBody messageBody = (TopologyMessageBody)message.Body;

        // set the neighbors
        TopologyNeighbors = messageBody.Topology![NodeId];
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

    // #region Broadcast
    // public Message HandleRead(Message message)
    // {
    //     return new Message()
    //     {
    //         Src = NodeId,
    //         Dest = message.Src,
    //         Body = new ReadMessageBody
    //         {
    //             Type = "read_ok",
    //             InReplyTo = message.Body.MsgId,
    //             Messages = this._messagesStored,
    //         }
    //     };
    // }
    // #endregion Broadcast

    // #region Grow-only Counter
    public Message HandleRead(Message message)
    {
        long result = 0;
        foreach (long delta in _deltaStore.Values)
        {
            result += delta;
        }
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new ReadMessageBody
            {
                Type = "read_ok",
                InReplyTo = message.Body.MsgId,
                Value = result,
            }
        };
    }
    // #endregion Grow-only Counter

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

    public Message HandleSyncDelta(Message message)
    {
        SyncDeltaMessageBody messageBody = (SyncDeltaMessageBody)message.Body;

        _deltaStore[message.Src] = messageBody.Detla;
        return new Message
        {
            Src = NodeId,
            Dest = NodeId,
        };
    }
}
