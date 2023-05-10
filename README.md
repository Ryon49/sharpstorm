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
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm broadcast --node-count 5 --time-limit 20 --rate 10 --log-stderr
```

## Grow-Only Counter
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w g-counter --bin ./sharpstorm/sharpstorm g-counter --node-count 3 --rate 100 --time-limit 20 --nemesis partition --log-stderr
```