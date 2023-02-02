using System.Net.Sockets;
using System.Text;

namespace Resharpis.Common;

public class ByteStreamWriter
{
    private byte[] Buffer { get; }

    public int Position { get; private set; }

    public ByteStreamWriter(int bufferSize)
    {
        Buffer = new byte[bufferSize];
    }

    public async Task Write(Socket socket)
    {
        var count = await socket.SendAsync(new ArraySegment<byte>(Buffer, 0, Position));
        Position -= count;
    }

    public void AddGetCommand(GetCommand getCommand)
    {
        Buffer[Position++] = 0x00;
        AddString(getCommand.Key);
    }

    public void AddGetResult(string result)
    {
        Buffer[Position++] = 0x00;
        AddString(result);
    }

    public void AddEmptyGetResult()
    {
        Buffer[Position++] = 0x00;
        BitConverter.TryWriteBytes(new Span<byte>(Buffer, Position, 4), 0);
        Position += 4;
    }

    public void AddSetCommand(SetCommand setCommand)
    {
        Buffer[Position++] = 0x01;
        AddString(setCommand.Key);
        AddString(setCommand.Value);
    }

    public void AddEmptySetResult()
    {
        Buffer[Position++] = 0x01;
        BitConverter.TryWriteBytes(new Span<byte>(Buffer, Position, 4), 0);
        Position += 4;
    }

    private void AddString(string value)
    {
        var bytes = Encoding.Unicode.GetBytes(value);
        var length = bytes.Length;

        BitConverter.TryWriteBytes(new Span<byte>(Buffer, Position, 4), length);
        Position += 4;
        Array.Copy(bytes, 0, Buffer, Position, length);
        Position += length;
    }
}