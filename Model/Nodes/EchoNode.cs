using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public class EchoNode : Node
{
    public EchoNode() : base()
    {
        _handlers[MessageType.ECHO] = HandleEcho;
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