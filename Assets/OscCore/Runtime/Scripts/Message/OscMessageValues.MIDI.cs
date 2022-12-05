using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single MIDI message element.
        /// Checks the element type before reading & returns default if it's not interpretable as a MIDI message.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MidiMessage ReadMidiElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.MIDI:
                    return *(MidiMessage*) (SharedBufferPtr + Offsets[index]);
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single MIDI message element, without checking the type tag of the element.
        /// Only call this if you are really sure that the element at the given index is a valid MIDI message,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MidiMessage ReadMidiElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            return *(MidiMessage*) (SharedBufferPtr + Offsets[index]);
        }
    }
}