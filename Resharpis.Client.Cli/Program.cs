using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Resharpis.Common;

var stopwatch = new Stopwatch();
var socket = new Socket(SocketType.Stream, ProtocolType.IP);
await socket.ConnectAsync(IPAddress.Any, 10_000);

Task.Run(async () =>
{
	await Task.Yield();

	var streamReader = new ByteStreamReader(4 * 1024);

	while (true)
	{
		await streamReader.Read(socket);
		stopwatch.Stop();
		Console.WriteLine($"Received set result after {stopwatch.Elapsed}");
	}
});

var streamWriter = new ByteStreamWriter(4 * 1024);
streamWriter.AddSetCommand(new SetCommand
{
	Key = "TEST",
	Value = "TEST"
});

stopwatch.Start();
await streamWriter.Write(socket);


Console.ReadKey(true);

int TryRead(int count, byte[] bytes)
{
	var size = 0;
	while (count > 4)
	{
		var length = BitConverter.ToInt32(bytes);
		var result = Encoding.ASCII.GetString(bytes, 4, length);
		Console.WriteLine($"Received {length} characters: <{result}>");

		Array.Copy(bytes, 4 + length, bytes, 0, count);
		count -= 4 + length;
		size += 4 + length;
	}

	return size;
}
