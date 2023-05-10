using System.Text.Json;
using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public abstract class Node
{
    // set-able, maybe.
    protected StreamReader _stdin = new StreamReader(Console.OpenStandardInput());
    protected StreamWriter _stdout = new StreamWriter(Console.OpenStandardOutput());
    protected StreamWriter _stderr = new StreamWriter(Console.OpenStandardError());

    public string NodeId = String.Empty;
    // store list of node ids that can communicate, regardless of topology
    public List<string> NodeIds = null!;
    // store list of node ids that is to the neighbor in the topology
    protected Dictionary<string, Func<Message, Message>> _handlers;

    public Node()
    {
        _handlers = new Dictionary<string, Func<Message, Message>>();
        _handlers[MessageType.INIT] = HandleInit;
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
            Message response = _handlers[request!.Body.Type](request);

            // Some message (like heartbeat) does not require a response 
            if (response.Src != response.Dest)
            {
                _stdout.WriteLine(JsonSerializer.Serialize(response));
                _stdout.Flush();
            }
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
}