using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Resharpis.Common;

await Task.Delay(1_000);

var waitHandle = new AutoResetEvent(false);
var stopwatch = new Stopwatch();
var tcpClient = new TcpClient();
await tcpClient.ConnectAsync(new IPEndPoint(IPAddress.IPv6Loopback, 10_000));

Task.Run(async () =>
{
	await Task.Yield();

	var streamReader = new ByteStreamReader(4 * 1024);

	while (true)
	{
		waitHandle.WaitOne();
		await streamReader.Read(tcpClient.Client);
		stopwatch.Stop();
		Console.WriteLine($"Received set result after {stopwatch.Elapsed}");

		if (streamReader.Length == 0)
		{
			break;
		}
	}
});

var streamWriter = new ByteStreamWriter(4 * 1024);
streamWriter.AddSetCommand(new SetCommand
{
	Key = "TEST",
	Value = "TEST"
});

Console.WriteLine($"Sending set command: {streamWriter.Position} bytes");

stopwatch.Start();
await streamWriter.Write(tcpClient.Client);
waitHandle.Set();

Console.ReadKey(true);

