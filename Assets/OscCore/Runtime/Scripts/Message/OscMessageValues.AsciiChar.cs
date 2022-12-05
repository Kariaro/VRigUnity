using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed partial class OscMessageValues
    {
        /// <summary>
        /// Read a non-standard ascii char element.
        /// Checks the element type before reading & returns default if it does not have the 'c' type tag
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The character value if the element has the right type tag, default otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadAsciiCharElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.AsciiChar32: 
                    // the ascii byte is placed at the end of the 4 bytes given for an element
                    return (char) m_SharedBuffer[Offsets[index] + 3];
                default: 
                    return default;
            }
        }
    }
}