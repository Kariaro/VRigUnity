using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed partial class OscMessageValues
    {
        /// <summary>
        /// Read a non-standard 'nil' or 'infinitum' tag, which has no value
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>True if the element's type tag is nil or infinitum, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadNilOrInfinitumElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.Infinitum:
                case TypeTag.Nil: return true;
                default: return default;
            }
        }
    }
}