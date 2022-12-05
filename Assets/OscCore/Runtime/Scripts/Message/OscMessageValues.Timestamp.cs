using MiniNtp;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single NTP timestamp message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a integer.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public NtpTimestamp ReadTimestampElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.TimeTag:
                    return NtpTimestamp.FromBigEndianBytes(SharedBufferPtr + Offsets[index]);
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single NTP timestamp message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid timestamp,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public NtpTimestamp ReadTimestampElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var ptr = SharedBufferPtr + Offsets[index];
            return NtpTimestamp.FromBigEndianBytes(ptr);
        }
        
        internal NtpTimestamp ReadTimestampIndex(int index)
        {
            var ptr = SharedBufferPtr + index;

            var bSeconds = *(uint*) ptr;
            // swap bytes from big to little endian 
            uint seconds = (bSeconds & 0x000000FFU) << 24 | (bSeconds & 0x0000FF00U) << 8 |
                           (bSeconds & 0x00FF0000U) >> 8 | (bSeconds & 0xFF000000U) >> 24;

            var bFractions = *(uint*) (ptr + 4);
            uint fractions = (bFractions & 0x000000FFU) << 24 | (bFractions & 0x0000FF00U) << 8 |
                             (bFractions & 0x00FF0000U) >> 8 | (bFractions & 0xFF000000U) >> 24;
            
            return new NtpTimestamp(seconds, fractions);
        }
    }
}