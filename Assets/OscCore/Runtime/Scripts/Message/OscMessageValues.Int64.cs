using System.Net;
using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 64-bit integer (long) message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a long.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public long ReadInt64Element(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Int64:
                    // TODO - optimize
                    long bigEndian = *(SharedBufferPtr + offset);
                    return IPAddress.NetworkToHostOrder(bigEndian);
                case TypeTag.Int32:
                    return m_SharedBuffer[offset    ] << 24 |
                           m_SharedBuffer[offset + 1] << 16 |
                           m_SharedBuffer[offset + 2] <<  8 |
                           m_SharedBuffer[offset + 3];
                case TypeTag.Float64:
                    m_SwapBuffer64[7] = m_SharedBuffer[offset];
                    m_SwapBuffer64[6] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer64[5] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer64[4] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer64[3] = m_SharedBuffer[offset + 4];
                    m_SwapBuffer64[2] = m_SharedBuffer[offset + 5];
                    m_SwapBuffer64[1] = m_SharedBuffer[offset + 6];
                    m_SwapBuffer64[0] = m_SharedBuffer[offset + 7];
                    double d = *SwapBuffer64Ptr;
                    return (long) d;
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    float f = *SwapBuffer32Ptr;
                    return (long) f;
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single 64-bit integer (long) message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid long,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64ElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif      
            long bigEndian = *(SharedBufferPtr + Offsets[index]);
            return IPAddress.NetworkToHostOrder(bigEndian);
        }
    }
}