using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single string message element.
        /// Checks the element type before reading & returns empty if it's not interpretable as a string.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public string ReadStringElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.AltTypeString: 
                case TypeTag.String:
                    var length = 0;
                    while (m_SharedBuffer[offset + length] != byte.MinValue) length++;
                    return Encoding.ASCII.GetString(m_SharedBuffer, offset, length);
                case TypeTag.Float64:
                    m_SwapBuffer64[7] = m_SharedBuffer[offset];
                    m_SwapBuffer64[6] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer64[5] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer64[4] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer64[3] = m_SharedBuffer[offset + 4];
                    m_SwapBuffer64[2] = m_SharedBuffer[offset + 5];
                    m_SwapBuffer64[1] = m_SharedBuffer[offset + 6];
                    m_SwapBuffer64[0] = m_SharedBuffer[offset + 7];
                    double f64 = *SwapBuffer64Ptr;
                    return f64.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    float f32 = *SwapBuffer32Ptr;
                    return f32.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Int64:
                    var i64 = IPAddress.NetworkToHostOrder(m_SharedBuffer[offset]);
                    return i64.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Int32:
                    int i32 = m_SharedBuffer[offset    ] << 24 |
                              m_SharedBuffer[offset + 1] << 16 |
                              m_SharedBuffer[offset + 2] <<  8 |
                              m_SharedBuffer[offset + 3];
                    return i32.ToString(CultureInfo.CurrentCulture);
                case TypeTag.False: 
                    return "False";
                case TypeTag.True: 
                    return "True";
                case TypeTag.Nil: 
                    return "Nil";
                case TypeTag.Infinitum: 
                    return "Infinitum";
                case TypeTag.Color32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    var color32 = *SwapBufferColor32Ptr;
                    return color32.ToString();
                case TypeTag.MIDI:
                    var midiPtr = SharedBufferPtr + offset;
                    var midi = *(MidiMessage*) midiPtr;
                    return midi.ToString();
                case TypeTag.AsciiChar32:
                    // ascii chars are encoded in the last byte of the 4-byte block
                    return ((char) m_SharedBuffer[offset + 3]).ToString();
                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Read a single string message element as bytes.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">The byte array to copy the string's bytes to</param>
        /// <returns>The byte length of the string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadStringElementBytes(int index, byte[] copyTo)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.AltTypeString:
                case TypeTag.String:
                    int i;
                    var offset = Offsets[index];
                    for (i = offset; i < m_SharedBuffer.Length; i++)
                    {
                        byte b = m_SharedBuffer[i];
                        if (b == byte.MinValue) break;
                        copyTo[i - offset] = b;
                    }
                    return i - offset;
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single string message element as bytes.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">The byte array to copy the string's bytes to</param>
        /// <param name="copyOffset">The index in the copyTo array to start copying at</param>
        /// <returns>The byte length of the string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadStringElementBytes(int index, byte[] copyTo, int copyOffset)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.AltTypeString:
                case TypeTag.String:
                    int i;
                    var offset = Offsets[index];
                    // when this is subtracted from i, it's the same as i - offset + copyOffset
                    var copyStartOffset = offset - copyOffset;
                    for (i = offset; i < m_SharedBuffer.Length; i++)
                    {
                        byte b = m_SharedBuffer[i];
                        if (b == byte.MinValue) break;
                        copyTo[i - copyStartOffset] = b;
                    }

                    return i - offset;
                default:
                    return default;
            }
        }
    }
}