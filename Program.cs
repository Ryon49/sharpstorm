using Sharpstone.Model.Nodes;

public class Program
{
    public static void Main(string[] args)
    {
        Node node = args[0] switch
        {
            "echo" => new EchoNode(),
            "unique-ids" => new GenerateUniqueIdsNode(),
            "broadcast" => new BroadcastNode(),
            "g-counter" => new GCounterNode(),
            _ => throw new NotImplementedException(),
        };
        node.Run();
    }
}