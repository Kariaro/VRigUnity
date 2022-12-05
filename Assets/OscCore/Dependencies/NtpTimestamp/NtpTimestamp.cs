using System;
using System.Runtime.CompilerServices;

namespace MiniNtp
{
    /// <summary>64-bit NTP timestamp as described in RFC-1305 & 5905</summary>
    public struct NtpTimestamp : IEquatable<NtpTimestamp>, IComparable<NtpTimestamp>
    {
        /// <summary>The number of seconds since the last epoch</summary>
        public readonly uint Seconds;
        
        /// <summary>Number of ~200 picosecond fractions of a second elapsed this second</summary>
        public readonly uint Fractions;

        public NtpTimestamp(uint seconds, uint fractions)
        {
            Seconds = seconds;
            Fractions = fractions;
        }

        public NtpTimestamp(DateTime dt)
        {
            var epoch = dt < TimeConstants.Epoch2036 ? TimeConstants.Epoch1900 : TimeConstants.Epoch2036;
            Seconds = (uint)(dt - epoch).TotalSeconds;
            Fractions = (uint)dt.Millisecond * TimeConstants.TimestampFractionsPerMs;
        }
        
        /// <summary>Read a new timestamp from big-endian bytes</summary>
        /// <param name="buffer">The array to read from</param>
        /// <param name="offset">The index in the array to start reading from</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NtpTimestamp FromBigEndianBytes(byte[] buffer, int offset)
        {
            fixed (byte* bPtr = &buffer[offset]) return FromBigEndianBytes((uint*) bPtr);
        }

        /// <summary>Read a new timestamp from big-endian bytes</summary>
        /// <param name="tsPtr">Pointer to the timestamp to read</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NtpTimestamp FromBigEndianBytes(byte* tsPtr)
        {
            return FromBigEndianBytes((uint*) tsPtr);
        }
        
        /// <summary>Read a new timestamp from big-endian bytes</summary>
        /// <param name="tsPtr">Pointer to the timestamp to read</param>
        public static unsafe NtpTimestamp FromBigEndianBytes(uint* tsPtr)
        {
            var bSeconds = *tsPtr;
            uint seconds = (bSeconds & 0x000000FFU) << 24 | (bSeconds & 0x0000FF00U) << 8 |
                           (bSeconds & 0x00FF0000U) >> 8 | (bSeconds & 0xFF000000U) >> 24;

            var bFractions = tsPtr[1];
            uint fractions = (bFractions & 0x000000FFU) << 24 | (bFractions & 0x0000FF00U) << 8 |
                             (bFractions & 0x00FF0000U) >> 8 | (bFractions & 0xFF000000U) >> 24;
            
            return new NtpTimestamp(seconds, fractions);
        }

        public DateTime ToDateTime()
        {
            // account for the special "now" value
            if (Fractions == 1 && Seconds == 0) return DateTime.Now;
            var epoch = DateTime.Now < TimeConstants.Epoch2036 ? TimeConstants.Epoch1900 : TimeConstants.Epoch2036;
            var fractionMs = Fractions * TimeConstants.FractionMillisecondMultiplier;
            var ms = (double) Seconds * 1000 + fractionMs;
            return epoch.AddMilliseconds(ms);
        }

        /// <summary>Convert to big-endian byte order</summary>
        /// <param name="bytes">The bytes to copy into</param>
        /// <param name="offset">The index in the bytes to start writing at</param>
        public unsafe void ToBigEndianBytes(byte[] bytes, int offset)
        {
            fixed (byte* bPtr = &bytes[offset])
            {
                var uiPtr = (uint*) bPtr;
                uiPtr[0] = Seconds.ReverseBytes();
                uiPtr[1] = Fractions.ReverseBytes();
            }
        }
        
        /// <summary>Convert to big-endian byte order</summary>
        /// <param name="writePtr">The pointer to start writing at</param>
        public unsafe void ToBigEndianBytes(uint* writePtr)
        {
            writePtr[0] = Seconds.ReverseBytes();
            writePtr[1] = Fractions.ReverseBytes();
        }

        public override string ToString()
        {
            return $"Seconds: {Seconds} , Fractions {Fractions}";
        }

        public bool Equals(NtpTimestamp other)
        {
            return Seconds == other.Seconds && Fractions == other.Fractions;
        }

        public override bool Equals(object obj)
        {
            return obj is NtpTimestamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Seconds * 397) ^ (int) Fractions;
            }
        }

        public static bool operator ==(NtpTimestamp left, NtpTimestamp right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NtpTimestamp left, NtpTimestamp right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(NtpTimestamp other)
        {
            var secondsComparison = Seconds.CompareTo(other.Seconds);
            return secondsComparison != 0 ? secondsComparison : Fractions.CompareTo(other.Fractions);
        }
        
        public static bool operator <(NtpTimestamp left, NtpTimestamp right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(NtpTimestamp left, NtpTimestamp right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(NtpTimestamp left, NtpTimestamp right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(NtpTimestamp left, NtpTimestamp right)
        {
            return left.CompareTo(right) >= 0;
        }
        
        // You can directly subtract ntp timestamps, but not add them
        public static NtpTimestamp operator -(NtpTimestamp left, NtpTimestamp right)
        {
            return new NtpTimestamp(left.Seconds - right.Seconds, left.Fractions - right.Fractions);
        }
    }
}