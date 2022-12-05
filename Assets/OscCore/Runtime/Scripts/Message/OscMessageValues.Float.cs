using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit float message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a float.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public float ReadFloatElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = SharedBufferPtr[offset + 3];
                    m_SwapBuffer32[1] = SharedBufferPtr[offset + 2];
                    m_SwapBuffer32[2] = SharedBufferPtr[offset + 1];
                    m_SwapBuffer32[3] = SharedBufferPtr[offset];
                    return *SwapBuffer32Ptr;
                case TypeTag.Int32:
                    return SharedBufferPtr[index    ] << 24 | 
                           SharedBufferPtr[index + 1] << 16 |
                           SharedBufferPtr[index + 2] <<  8 |
                           SharedBufferPtr[index + 3];
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single 32-bit float message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid float,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            m_SwapBuffer32[0] = SharedBufferPtr[offset + 3];
            m_SwapBuffer32[1] = SharedBufferPtr[offset + 2];
            m_SwapBuffer32[2] = SharedBufferPtr[offset + 1];
            m_SwapBuffer32[3] = SharedBufferPtr[offset];
            return *SwapBuffer32Ptr;
        }
    }
}