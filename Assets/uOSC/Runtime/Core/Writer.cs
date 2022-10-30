using System;
using System.IO;
using System.Text;

namespace uOSC
{

public static class Writer
{
    private static readonly byte Zero = Convert.ToByte('\0');
    public static readonly byte[] Zeros = { Zero, Zero, Zero, Zero };

    public static void Write(MemoryStream stream, int value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    public static void Write(MemoryStream stream, Timestamp value)
    {
        var byteValue = BitConverter.GetBytes(value.value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    public static void Write(MemoryStream stream, float value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    public static void Write(MemoryStream stream, string value)
    {
        var byteValue = Encoding.UTF8.GetBytes(value);
        var size = byteValue.Length;
        stream.Write(byteValue, 0, size);

        var offset = Util.GetStringAlignedSize(size) - size;
        if (offset > 0)
        {
            stream.Write(Zeros, 0, offset);
        }
    }

    public static void Write(MemoryStream stream, byte[] value, int size)
    {
        Write(stream, size);
        stream.Write(value, 0, size);

        var offset = Util.GetBufferAlignedSize(size) - size;
        if (offset > 0)
        {
            stream.Write(Zeros, 0, offset);
        }
    }

    public static void Write(MemoryStream stream, byte[] value)
    {
        Write(stream, value, value.Length);
    }

    public static void Write(MemoryStream stream, MemoryStream value)
    {
        var byteValue = Util.GetBuffer(value);
        var size = (int)value.Position;
        Write(stream, size);
        stream.Write(byteValue, 0, size);
    }
}

}