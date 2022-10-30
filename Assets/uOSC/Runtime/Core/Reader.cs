using System;
using System.Text;

namespace uOSC
{

public static class Reader
{
    public static string ParseString(byte[] buf, ref int pos)
    {
        int size = 0;
        int bufSize = buf.Length;
        for (; buf[pos + size] != 0; ++size);
        var value = Encoding.UTF8.GetString(buf, pos, size);
        pos += Util.GetStringAlignedSize(size);
        return value;
    }

    public static int ParseInt(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToInt32(buf, pos);
        pos += 4;
        return value;
    }

    public static float ParseFloat(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToSingle(buf, pos);
        pos += 4;
        return value;
    }

    public static byte[] ParseBlob(byte[] buf, ref int pos)
    {
        var size = ParseInt(buf, ref pos);
        var value = new byte[size];
        Buffer.BlockCopy(buf, pos, value, 0, size);
        pos += Util.GetBufferAlignedSize(size);
        return value;
    }

    public static ulong ParseTimetag(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 8);
        var value = BitConverter.ToUInt64(buf, pos);
        pos += 8;
        return value;
    }
}

}