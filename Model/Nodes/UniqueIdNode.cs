
using Sharpstone.Model.Messages;

namespace Sharpstone.Model.Nodes;

public class GenerateUniqueIdsNode : Node
{
    public GenerateUniqueIdsNode() : base()
    {
        _handlers[MessageType.GENERATE] = HandleGenerateUniqueId;
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
}