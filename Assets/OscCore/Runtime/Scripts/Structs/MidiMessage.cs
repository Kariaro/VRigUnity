using System;
using System.Runtime.InteropServices;

namespace OscCore
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MidiMessage : IEquatable<MidiMessage>
    {
        [FieldOffset(0)] readonly int data;
        
        [FieldOffset(0)] public readonly byte PortId;
        [FieldOffset(1)] public readonly byte Status;
        [FieldOffset(2)] public readonly byte Data1;
        [FieldOffset(3)] public readonly byte Data2;

        public MidiMessage(byte[] bytes, int offset)
        {
            data = 0;
            PortId = bytes[offset];
            Status = bytes[offset + 1];
            Data1 = bytes[offset + 2];
            Data2 = bytes[offset + 3];
        }
        
        public MidiMessage(byte portId, byte status, byte data1, byte data2)
        {
            data = 0;
            PortId = portId;
            Status = status;
            Data1 = data1;
            Data2 = data2;
        }

        public override string ToString()
        {
            return $"Port ID: {PortId}, Status: {Status}, Data 1: {Data1} , 2: {Data2}";
        }

        public bool Equals(MidiMessage other)
        {
            return PortId == other.PortId && Status == other.Status && Data1 == other.Data1 && Data2 == other.Data2;
        }

        public override bool Equals(object obj)
        {
            return obj is MidiMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Status.GetHashCode();
                hashCode = (hashCode * 397) ^ Data1.GetHashCode();
                hashCode = (hashCode * 397) ^ Data2.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MidiMessage left, MidiMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MidiMessage left, MidiMessage right)
        {
            return !left.Equals(right);
        }
    }
}

