# Sharpstorm
A C# implementation of Gossip Glomers Challenge.

## Echo
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w echo --bin ./sharpstorm/sharpstorm --node-count 1 --time-limit 10 --log-stderr
```

## Generate Unique Id
Use UUID as unique id.
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w unique-ids --bin ./sharpstorm/sharpstorm --time-limit 30 --rate 1000 --node-count 3 --availability total --nemesis partition --log-stderr
```

## Broadcast
``` shell
dotnet publish -o sharpstorm; ./maelstrom/maelstrom test -w broadcast --bin ./sharpstorm/sharpstorm --node-count 1 --time-limit 20 --rate 10 --log-stderr
```