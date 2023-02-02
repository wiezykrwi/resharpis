using System.Net.Sockets;
using System.Text;

namespace Resharpis.Common;

public class ByteStreamReader
{
    public byte[] Buffer { get; }

    public int Position { get; private set; }
    public int Length { get; private set; }

    public ByteStreamReader(int bufferSize)
    {
        Buffer = new byte[bufferSize];
    }

    public async Task<bool> Read(Socket socket)
    {
        var memory = new ArraySegment<byte>(Buffer, Length, 1024);
        var count = await socket.ReceiveAsync(memory);
        Length += count;
        return count != 0;
    }

    public byte GetByte()
    {
        return Buffer[Position++];
    }

    public int GetInt32()
    {
        var result = BitConverter.ToInt32(new Span<byte>(Buffer, Position, 4));
        Position += 4;
        return result;
    }

    public string GetString()
    {
        var length = GetInt32();
        var result = Encoding.Unicode.GetString(Buffer, Position, length);
        Position += length;
        return result;
    }
}