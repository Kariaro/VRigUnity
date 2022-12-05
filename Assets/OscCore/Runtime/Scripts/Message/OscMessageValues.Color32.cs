using System.Runtime.CompilerServices;
using UnityEngine;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit RGBA color message element.
        /// Checks the element type before reading & returns default if it's not interpretable as a color.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public Color32 ReadColor32Element(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Color32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    return *SwapBufferColor32Ptr;
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single 32-bit RGBA color message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid Color32,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color32 ReadColor32ElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
            m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
            m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
            m_SwapBuffer32[3] = m_SharedBuffer[offset];
            return *SwapBufferColor32Ptr;
        }
    }
}