using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit integer message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a integer.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public int ReadIntElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Int32:
                    return m_SharedBuffer[offset    ] << 24 |
                           m_SharedBuffer[offset + 1] << 16 |
                           m_SharedBuffer[offset + 2] <<  8 |
                           m_SharedBuffer[offset + 3];
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    float f = *SwapBuffer32Ptr;
                    return (int) f;
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single 32-bit int message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid integer,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadIntElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif            
            var offset = Offsets[index];
            return m_SharedBuffer[offset    ] << 24 |
                   m_SharedBuffer[offset + 1] << 16 |
                   m_SharedBuffer[offset + 2] <<  8 |
                   m_SharedBuffer[offset + 3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint ReadUIntIndex(int index)
        {
            m_SwapBuffer32[0] = m_SharedBuffer[index + 3];
            m_SwapBuffer32[1] = m_SharedBuffer[index + 2];
            m_SwapBuffer32[2] = m_SharedBuffer[index + 1];
            m_SwapBuffer32[3] = m_SharedBuffer[index];
            return *SwapBuffer32UintPtr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int ReadIntIndex(int index)
        {
            return m_SharedBuffer[index    ] << 24 |
                   m_SharedBuffer[index + 1] << 16 |
                   m_SharedBuffer[index + 2] << 8 |
                   m_SharedBuffer[index + 3];
        }
    }
}