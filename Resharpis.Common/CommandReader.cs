namespace Resharpis.Common;

public class CommandReader
{
    public GetCommand ReadGetCommand(ByteStreamReader streamReader)
    {
        return new GetCommand
        {
            Key = streamReader.GetString()
        };
    }

    public SetCommand ReadSetCommand(ByteStreamReader streamReader)
    {
        return new SetCommand
        {
            Key = streamReader.GetString(),
            Value = streamReader.GetString()
        };
    }
}