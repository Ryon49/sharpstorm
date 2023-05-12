# Sharpstorm
A C# implementation of Gossip Glomers Challenge.

## Echo
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w echo --bin ./sharpstorm/sharpstorm echo --node-count 1 --time-limit 10 --log-stderr
```

## Generate Unique Id
Use UUID as unique id.
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w unique-ids --bin ./sharpstorm/sharpstorm unique-ids --time-limit 30 --rate 1000 --node-count 3 --availability total --nemesis partition --log-stderr
```

## Broadcast
Single-Node Broadcast
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm broadcast --node-count 1 --time-limit 20 --rate 10 --log-stderr
```
Multi-Node Broadcast
- So here is the idea, the passive approach.
    Each node record the number of messages other node has. And for every X second, the node gossip/ask its neighbor about new messages
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm broadcast --node-count 5 --time-limit 20 --rate 10 --log-stderr
```
Fault Tolerant Broadcast
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm broadcast --node-count 5 --time-limit 20 --rate 10 --nemesis partition --log-stderr
```
Efficient Broadcast, Part I & II
- Due to the passive nature, 
    X = 25ms, with 60 messages per opersion, the latency is close to 5724ms
    X = 500ms, with 4 messages per opersion, the latency is close to 16893ms
    X = 1s, with 4 messages per opersion, the latency is close to 11557ms
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm broadcast --node-count 25 --time-limit 20 --rate 100 --latency 100 --log-stderr
```

## Grow-Only Counter
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w g-counter --bin ./sharpstorm/sharpstorm g-counter --node-count 3 --rate 100 --time-limit 20 --nemesis partition --log-stderr
```