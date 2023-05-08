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

            _stdout.WriteLine(JsonSerializer.Serialize(response));
            _stdout.Flush();
        }
    }

    public Message HandleInit(Message message)
    {
        InitMessageBody initMessageBody = (InitMessageBody)message.Body;
        NodeId = initMessageBody.NodeId;
        NodeIds = initMessageBody.NodeIds;
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

    public Message HandleRead(Message message)
    {
        return new Message()
        {
            Src = NodeId,
            Dest = message.Src,
            Body = new ReadMessageBody
            {
                Type = "read_ok",
                InReplyTo = message.Body.MsgId,
                Messages = this._messagesStored,
            }
        };
    }
}
