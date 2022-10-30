using UnityEngine;
using System;
using System.IO;

namespace uOSC
{

public static class Util
{
    public static bool IsMultipleOfFour(int num)
    {
        return num == (num & ~0x3);
    }

    public static int GetStringAlignedSize(int size)
    {
        return (size + 4) & ~0x3;
    }

    public static int GetBufferAlignedSize(int size)
    {
        var offset = size & ~0x3;
        return (offset == size) ? size : (offset + 4);
    }

    public static string GetString(this object value)
    {
        if (value is int)    return ((int)value).ToString();
        if (value is float)  return ((float)value).ToString();
        if (value is string) return (string)value;
        if (value is byte[]) return "Byte[" + ((byte[])value).Length + "]";

        return value.ToString();
    }

    public static byte[] GetBuffer(MemoryStream stream)
    {
#if NETFX_CORE
        ArraySegment<byte> buffer;
        if (!stream.TryGetBuffer(out buffer))
        {
            Debug.LogError("Failed to perform MemoryStream.TryGetBuffer()");
            return null;
        }
        return buffer.Array;
#else
        return stream.GetBuffer();
#endif
    }
}

}