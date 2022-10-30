using UnityEngine;
using System.Collections.Generic;

namespace uOSC
{

public static class Identifier
{
    public const string Bundle = "#bundle";

    public const char Int    = 'i';
    public const char Float  = 'f';
    public const char String = 's';
    public const char Blob   = 'b';
    public const char True   = 'T';
    public const char False  = 'F';
}

public class Parser
{
    public static readonly object[] EmptyObjectArray = new object[0];

    object lockObject_ = new object();
    Queue<Message> messages_ = new Queue<Message>();

    public int messageCount
    {
        get { return messages_.Count; }
    }

    public void Parse(byte[] buf, ref int pos, int endPos, ulong timestamp = 0x1u)
    {
        var first = Reader.ParseString(buf, ref pos);

        if (first == Identifier.Bundle)
        {
            ParseBundle(buf, ref pos, endPos);
        }
        else
        {
            var values = ParseData(buf, ref pos);
            lock (lockObject_)
            {
                messages_.Enqueue(new Message() 
                {
                    address = first,
                    timestamp = new Timestamp(timestamp),
                    values = values
                });
            }
        }

        if (pos != endPos)
        {
            Debug.LogErrorFormat(
                "The parsed data size is inconsitent with the given size: {0} / {1}", 
                pos,
                endPos);
        }
    }

    public Message Dequeue()
    {
        if (messageCount == 0)
        {
            return Message.none;
        }

        lock (lockObject_)
        {
            return messages_.Dequeue();
        }
    }

    void ParseBundle(byte[] buf, ref int pos, int endPos)
    {
        var time = Reader.ParseTimetag(buf, ref pos);

        while (pos < endPos)
        {
            var contentSize = Reader.ParseInt(buf, ref pos);
            if (Util.IsMultipleOfFour(contentSize))
            {
                Parse(buf, ref pos, pos + contentSize, time);
            }
            else
            {
                Debug.LogErrorFormat("Given data is invalid (bundle size ({0}) is not a multiple of 4).", contentSize);
                pos += contentSize;
            }
        }
    }

    object[] ParseData(byte[] buf, ref int pos)
    {
        // remove ','
        var types = Reader.ParseString(buf, ref pos).Substring(1);

        var n = types.Length;
        if (n == 0) return EmptyObjectArray;

        var data = new object[n];

        for (int i = 0; i < n; ++i)
        {
            switch (types[i])
            {
                case Identifier.Int    : data[i] = Reader.ParseInt(buf, ref pos); break;
                case Identifier.Float  : data[i] = Reader.ParseFloat(buf, ref pos); break;
                case Identifier.String : data[i] = Reader.ParseString(buf, ref pos); break;
                case Identifier.Blob   : data[i] = Reader.ParseBlob(buf, ref pos); break;
                case Identifier.True   : data[i] = true; break;
                case Identifier.False  : data[i] = false; break;
                default:
                    // Add more types here if you want to handle them.
                    break;
            }
        }

        return data;
    }
}

}