using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

// allow tests to modify things as if in the same assembly
[assembly:InternalsVisibleTo("OscCore.Tests.Editor")]

namespace OscCore
{
    /// <summary>
    /// Represents the tags and values associated with a received OSC message
    /// </summary>
    public sealed unsafe partial class OscMessageValues
    {
        // the buffer where we read messages from - usually provided + filled by a socket reader
        readonly byte[] m_SharedBuffer;
        readonly byte* SharedBufferPtr;
        // used to swap bytes for 32-bit numbers when reading
        readonly byte[] m_SwapBuffer32 = new byte[4];
        readonly float* SwapBuffer32Ptr;
        readonly uint* SwapBuffer32UintPtr;
        readonly Color32* SwapBufferColor32Ptr;
        readonly GCHandle m_Swap32Handle;
        // used to swap bytes for 64-bit numbers when reading
        readonly byte[] m_SwapBuffer64 = new byte[8];
        readonly double* SwapBuffer64Ptr;
        readonly GCHandle m_Swap64Handle;

        /// <summary>
        /// All type tags in the message.
        /// All values past index >= ElementCount are junk data and should NEVER BE USED!
        /// </summary>
        internal readonly TypeTag[] Tags;
        
        /// <summary>
        /// Indexes into the shared buffer associated with each message element
        /// All values at index >= ElementCount are junk data and should NEVER BE USED!
        /// </summary>
        internal readonly int[] Offsets;
        
        /// <summary>The number of elements in the OSC Message</summary>
        public int ElementCount { get; internal set; }

        internal OscMessageValues(byte[] buffer, int elementCapacity = 8)
        {
            ElementCount = 0;
            Tags = new TypeTag[elementCapacity];
            Offsets = new int[elementCapacity];
            m_SharedBuffer = buffer;

            fixed (byte* bufferPtr = buffer) { SharedBufferPtr = bufferPtr; }

            // pin byte swap buffers in place, so that we can count on the pointers never changing
            m_Swap32Handle = GCHandle.Alloc(m_SwapBuffer32, GCHandleType.Pinned);
            var swap32Ptr = m_Swap32Handle.AddrOfPinnedObject();
            SwapBuffer32Ptr = (float*) swap32Ptr;
            SwapBuffer32UintPtr = (uint*) swap32Ptr;
            SwapBufferColor32Ptr = (Color32*) (byte*) swap32Ptr;
            
            SwapBuffer64Ptr = Utils.PinPtr<byte, double>(m_SwapBuffer64, out m_Swap64Handle);
        }

        ~OscMessageValues()
        {
            m_Swap32Handle.Free();
            m_Swap64Handle.Free();
        }

        /// <summary>Execute a method for every element in the message</summary>
        /// <param name="elementAction">A method that takes in the index and type tag for an element</param>
        public void ForEachElement(Action<int, TypeTag> elementAction)
        {
            for (int i = 0; i < ElementCount; i++)
                elementAction(i, Tags[i]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool OutOfBounds(int index)
        {
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return true;
            }

            return false;
        }
    }
}