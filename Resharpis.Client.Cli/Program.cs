using System.Net;
using System.Net.Sockets;
using System.Text;

var socket = new Socket(SocketType.Stream, ProtocolType.IP);

socket.Connect(IPAddress.Any, 10_000);

var message = "Hello World!";
var payload = Encoding.ASCII.GetBytes(message);
var packet = new byte[2 * 1024];
var packetSize = 0;
while ((packetSize + 4 + payload.Length) < packet.Length)
{
	BitConverter.TryWriteBytes(new Span<byte>(packet, packetSize, 4), payload.Length);
	Array.Copy(payload, 0, packet, 4 + packetSize, payload.Length);
	packetSize += 4 + payload.Length;
}

Console.WriteLine($"Sending {packetSize} bytes");
Console.WriteLine(string.Join(" ", packet.Take(packetSize).Select(x => x.ToString("X2"))));
socket.Send(new Span<byte>(packet, 0, packetSize));

Task.Run(async () =>
{
	await Task.Yield();

	var buffer = new byte[4*1024];
	var position = 0;

	while (true)
	{
		var count = socket.Receive(new Span<byte>(buffer, position, buffer.Length - position));
		position += count;
		if (position < 4) continue;

		Console.WriteLine($"Received {position} bytes");
		Console.WriteLine(string.Join(" ", buffer.Take(position).Select(x => x.ToString("X2"))));
		
		while (position > 4)
		{
			var length = BitConverter.ToInt32(buffer);
			var result = Encoding.ASCII.GetString(buffer, 4, length);
			Console.WriteLine($"Received {length} characters: <{result}>");

			Array.Copy(buffer, 4 + length, buffer, 0, position);
			position -= 4 + length;
		}
	}
});

Console.ReadKey(true);
