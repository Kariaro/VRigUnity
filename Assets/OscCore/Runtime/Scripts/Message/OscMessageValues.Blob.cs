using System;
using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        const int k_ResizeByteHeadroom = 1024;    
        
        /// <summary>
        /// Read a blob element.
        /// Checks the element type before reading, and does nothing if the element is not a blob.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">
        /// The array to copy blob contents into.
        /// Will be resized if it lacks sufficient capacity
        /// </param>
        /// <param name="copyOffset">The index in the copyTo array to start copying at</param>
        /// <returns>The size of the blob if valid, 0 otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadBlobElement(int index, ref byte[] copyTo, int copyOffset = 0)
        {
#if OSCCORE_SAFETY_CHECKS
            if (OutOfBounds(index)) return default;
#endif
            switch (Tags[index])
            {
                case TypeTag.Blob:
                    var offset = Offsets[index];
                    var size = ReadIntIndex(offset);
                    var dataStart = offset + 4;    // skip the size int
                    if (copyTo.Length - copyOffset <= size)
                        Array.Resize(ref copyTo, size + copyOffset + k_ResizeByteHeadroom);

                    Buffer.BlockCopy(m_SharedBuffer, dataStart, copyTo, copyOffset, size);
                    return size;
                default: 
                    return default;
            }
        }

    }
}