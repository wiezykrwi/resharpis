using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Resharpis.Common;

var cache = new ConcurrentDictionary<string, string>();
var commandReader = new CommandReader();

Task.Run(async () =>
{
	await Task.Yield();

	var tcpListener = new TcpListener(new IPEndPoint(IPAddress.IPv6Loopback, 10_000));
	tcpListener.Start(100);
	Console.WriteLine($"Listening for connections on {tcpListener.LocalEndpoint}");

	while (true)
	{
		var connection = await tcpListener.AcceptSocketAsync();
		Task.Run(() => ProcessConnection(connection));
	}
}).ConfigureAwait(false);

Console.WriteLine($"Press any key to stop...");
Console.ReadKey(true);

async Task ProcessConnection(Socket remoteSocket)
{
	Console.WriteLine($"[{remoteSocket.RemoteEndPoint}] Accepted incoming connection");

	var streamReader = new ByteStreamReader(4 * 1024);
	var streamWriter = new ByteStreamWriter(4 * 1024);

	while (true)
	{
		if (!await streamReader.Read(remoteSocket))
		{
			Console.WriteLine($"[{remoteSocket.RemoteEndPoint}] Disconnected");
			return;
		}
		
		Console.WriteLine($"[{remoteSocket.RemoteEndPoint}] Received {streamReader.Length} bytes");

		while (streamReader.Position < streamReader.Length)
		{
			var operation = streamReader.GetByte();
			switch (operation)
			{
				case 0x00:
					var getCommand = commandReader.ReadGetCommand(streamReader);
					if (cache.TryGetValue(getCommand.Key.ToUpper(), out var value))
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
					cache.AddOrUpdate(setCommand.Key.ToUpper(), _ => setCommand.Value, (_, _) => setCommand.Value);
					streamWriter.AddEmptySetResult();
					break;
			}
			
			Console.WriteLine($"[{remoteSocket.RemoteEndPoint}] Sending result {streamWriter.Position} bytes");
			await streamWriter.Write(remoteSocket);
		}
	}
}
