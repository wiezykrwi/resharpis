using System.Net;
using System.Net.Sockets;

var cache = new Dictionary<string, string>();

using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
socket.Bind(new IPEndPoint(IPAddress.Any, 10_000));
socket.Listen(100);

Task.Run(async () =>
{
	await Task.Yield();

	while (true)
	{
		var connection = await socket.AcceptAsync();
		Task.Run(() => ProcessConnection(connection));
	}
}).ConfigureAwait(false);

Console.WriteLine($"Listening for connections on {socket.LocalEndPoint}");
Console.WriteLine($"Press any key to stop...");
Console.ReadKey(true);

async Task ProcessConnection(Socket socket)
{
	Console.WriteLine($"Accepted incoming connection from {socket.RemoteEndPoint}");

	var position = 0;
	var buffer = new byte[4*1024];

	while (true)
	{
		var memory = new ArraySegment<byte>(buffer, position, 1024);
		var count = await socket.ReceiveAsync(memory);
		Console.WriteLine($"Received {count} bytes");
		position += count;

		while (position > 4)
		{
			var length = BitConverter.ToInt32(buffer);
			if (position < 4 + length)
			{
				break;
			}

			var messageBuffer = new byte[4 + length];
			BitConverter.TryWriteBytes(messageBuffer, length);
			Array.Copy(buffer, 4, messageBuffer, 4, length);
			await socket.SendAsync(messageBuffer);

			Array.Copy(buffer, 4 + length, buffer, 0, position);
			position -= 4 + length;
		}
	}
}
