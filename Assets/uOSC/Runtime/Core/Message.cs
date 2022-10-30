using System.IO;
using System.Text;

namespace uOSC
{

public struct Message
{
    public string address;
    public Timestamp timestamp;
    public object[] values;

    public static Message none
    {
        get { return new Message(""); } 
    }

    public Message(string address, params object[] packet)
    {
        this.address = address;
        this.timestamp = new Timestamp();
        this.values = packet;
    }

    public void Write(MemoryStream stream)
    {
        WriteAddress(stream);
        WriteTypes(stream);
        WriteValues(stream);
    }

    void WriteAddress(MemoryStream stream)
    {
        Writer.Write(stream, address);
    }

    void WriteTypes(MemoryStream stream)
    {
        string types = ",";
        for (int i = 0; i < values.Length; ++i)
        {
            var value = values[i];
            if      (value is int)    types += Identifier.Int;
            else if (value is float)  types += Identifier.Float;
            else if (value is string) types += Identifier.String;
            else if (value is byte[]) types += Identifier.Blob;
            else if (value is bool)   types += (bool)value ? Identifier.True : Identifier.False;
        }
        Writer.Write(stream, types);
    }

    void WriteValues(MemoryStream stream)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            var value = values[i];
            if      (value is int)    Writer.Write(stream, (int)value);
            else if (value is float)  Writer.Write(stream, (float)value);
            else if (value is string) Writer.Write(stream, (string)value);
            else if (value is byte[]) Writer.Write(stream, (byte[])value);
        }
    }

    public override string ToString()
    {
        var str = new StringBuilder();

        str.Append(address);
        str.Append("\t");

        foreach (var value in values)
        {
            str.Append(value.GetString());
            str.Append(" ");
        }

        str.Append($"({timestamp.ToLocalTime()})");

        return str.ToString();
    }
}

}