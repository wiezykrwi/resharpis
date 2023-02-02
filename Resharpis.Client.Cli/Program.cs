using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Resharpis.Common;

var tcpClient = new TcpClient();
await tcpClient.ConnectAsync(new IPEndPoint(IPAddress.IPv6Loopback, 10_000));

var streamReader = new ByteStreamReader(4 * 1024);
var streamWriter = new ByteStreamWriter(4 * 1024);
var inputRegex = new Regex(@"(?<COMMAND_TYPE>[A-Za-z]{3}) (?<COMMAND>.*)");
var getRegex = new Regex(@"(?<KEY>[A-Za-z]+)");
var setRegex = new Regex(@"(?<KEY>[A-Za-z]+) (?<VALUE>[A-Za-z]+)");

while (true)
{
	Console.ResetColor();
	Console.Write("> ");
	var input = Console.ReadLine();
	if (string.IsNullOrWhiteSpace(input))
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("Invalid input");
		continue;
	}
	
	var inputMatch = inputRegex.Match(input);
	if (!inputMatch.Success) 
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("Invalid input");
		continue;
	}

	switch (inputMatch.Groups["COMMAND_TYPE"].Value.ToUpper())
	{
		case "GET":
			var getMatch = getRegex.Match(inputMatch.Groups["COMMAND"].Value);
			if (!getMatch.Success) 
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Invalid get command");
				continue;
			}
			
			streamWriter.AddGetCommand(new GetCommand
			{
				Key = getMatch.Groups["KEY"].Value.ToUpper()
			});
			break;
		case "SET":
			var setMatch = setRegex.Match(inputMatch.Groups["COMMAND"].Value);
			if (!setMatch.Success) 
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Invalid set command");
				continue;
			}

			streamWriter.AddSetCommand(new SetCommand
			{
				Key = setMatch.Groups["KEY"].Value.ToUpper(),
				Value = setMatch.Groups["VALUE"].Value
			});
			break;
		
		default:
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Invalid command");
			continue;
	}
	
	Console.WriteLine($"Sending command: {streamWriter.Position} bytes");
	var stopwatch = Stopwatch.StartNew();
	await streamWriter.Write(tcpClient.Client);
	await streamReader.Read(tcpClient.Client);
	stopwatch.Stop();
	if (streamReader.Position < streamReader.Length)
	{
		var operation = streamReader.GetByte();
		switch (operation)
		{
			case 0x00:
				var result = streamReader.GetString();
				Console.WriteLine($"Received value: {result} ({stopwatch.Elapsed})");
				break;
			case 0x01:
				streamReader.GetInt32();
				Console.WriteLine($"OK ({stopwatch.Elapsed})");
				break;
		}
	}
}