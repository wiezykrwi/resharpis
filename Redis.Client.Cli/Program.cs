using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

var stopwatch = Stopwatch.StartNew();
var socket = new Socket(SocketType.Stream, ProtocolType.IP);

socket.Connect(IPAddress.Any, 10_000);

var message = "Hello World!";
var payload = Encoding.ASCII.GetBytes(message);
var packet = new byte[4 + payload.Length];
BitConverter.TryWriteBytes(packet, payload.Length);
Array.Copy(payload, 0, packet, 4, payload.Length);

socket.Send(packet);

var buffer = new byte[4*1024];

var count = socket.Receive(buffer);

var length = BitConverter.ToInt32(buffer);
var result = Encoding.ASCII.GetString(buffer, 4, length);
stopwatch.Stop();
Console.WriteLine($"Received {length} characters: <{result}> after {stopwatch.Elapsed}");
