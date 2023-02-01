using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Resharpis.Common;

var cache = new ConcurrentDictionary<string, string>();
var commandReader = new CommandReader();

Task.Run(async () =>
{
	await Task.Yield();
	
	using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
	socket.Bind(new IPEndPoint(IPAddress.Any, 10_000));
	socket.Listen(100);
	Console.WriteLine($"Listening for connections on {socket.LocalEndPoint}");

	while (true)
	{
		var connection = await socket.AcceptAsync();
		Task.Run(() => ProcessConnection(connection));
	}
}).ConfigureAwait(false);

Console.WriteLine($"Press any key to stop...");
Console.ReadKey(true);

async Task ProcessConnection(Socket remoteSocket)
{
	Console.WriteLine($"Accepted incoming connection from {remoteSocket.RemoteEndPoint}");

	var streamReader = new ByteStreamReader(4 * 1024);
	var streamWriter = new ByteStreamWriter(4 * 1024);

	while (true)
	{
		await streamReader.Read(remoteSocket);

		while (streamReader.Length > 1)
		{
			var operation = streamReader.GetByte();
			switch (operation)
			{
				case 0x00:
					var getCommand = commandReader.ReadGetCommand(streamReader);
					if (cache.TryGetValue(getCommand.Key, out var value))
					{
						streamWriter.AddGetResult(value);
					}
					else
					{
						streamWriter.AddEmptyGetResult();
					}

					break;
				
				case 0x01:
					var setCommand = commandReader.ReadSetCommand(streamReader);
					cache.AddOrUpdate(setCommand.Key, _ => setCommand.Value, (_, _) => setCommand.Value);
					streamWriter.AddEmptySetResult();
					break;
			}
			
			await streamWriter.Write(remoteSocket);
		}
	}
}
