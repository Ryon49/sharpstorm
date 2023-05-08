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
    public List<string> NodeIds = null!;

    private Dictionary<string, Func<Message, Message>> _handlers;

    public Node()
    {
        _handlers = new Dictionary<string, Func<Message, Message>>()
        {
            {nameof(InitMessageBody), HandleInit},
            {nameof(EchoMessageBody), HandleEcho},
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
}