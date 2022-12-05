using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlobHandles;
using MiniNtp;
using UnityEngine;

namespace OscCore
{
    public sealed unsafe class OscWriter : IDisposable
    {
        public readonly byte[] Buffer;
        readonly byte* m_BufferPtr;
        readonly GCHandle m_BufferHandle;
        readonly MidiMessage* m_BufferMidiPtr;
        
        readonly float[] m_FloatSwap = new float[1];
        readonly byte* m_FloatSwapPtr;
        readonly GCHandle m_FloatSwapHandle;
        
        readonly double[] m_DoubleSwap = new double[1];
        readonly byte* m_DoubleSwapPtr;
        readonly GCHandle m_DoubleSwapHandle;

        readonly Color32[] m_Color32Swap = new Color32[1];
        readonly byte* m_Color32SwapPtr;
        readonly GCHandle m_Color32SwapHandle;

        int m_Length;
        
        /// <summary>The number of bytes currently written to the buffer</summary>
        public int Length => m_Length;
        
        public OscWriter(int capacity = 4096)
        {
            Buffer = new byte[capacity];

            // Even though Unity's GC does not move objects around, pin them to be safe.
            m_BufferPtr = Utils.PinPtr<byte, byte>(Buffer, out m_BufferHandle);
            m_BufferMidiPtr = (MidiMessage*) m_BufferPtr;

            m_FloatSwapPtr = Utils.PinPtr<float, byte>(m_FloatSwap, out m_FloatSwapHandle);
            m_DoubleSwapPtr = Utils.PinPtr<double, byte>(m_DoubleSwap, out m_DoubleSwapHandle);
            m_Color32SwapPtr = Utils.PinPtr<Color32, byte>(m_Color32Swap, out m_Color32SwapHandle);
        }

        ~OscWriter() { Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() { m_Length = 0; }

        /// <summary>Write a 32-bit integer element</summary>
        public void Write(int data)
        {
            Buffer[m_Length++] = (byte) (data >> 24);
            Buffer[m_Length++] = (byte) (data >> 16);
            Buffer[m_Length++] = (byte) (data >>  8);
            Buffer[m_Length++] = (byte) (data);
        }
        
        /// <summary>Write a 32-bit floating point element</summary>
        public void Write(float data)
        {
            m_FloatSwap[0] = data;
            Buffer[m_Length++] = m_FloatSwapPtr[3];
            Buffer[m_Length++] = m_FloatSwapPtr[2];
            Buffer[m_Length++] = m_FloatSwapPtr[1];
            Buffer[m_Length++] = m_FloatSwapPtr[0];
        }
        
        /// <summary>Write a 2D vector as two float elements</summary>
        public void Write(Vector2 data)
        {
            Write(data.x);
            Write(data.y);
        }
        
        /// <summary>Write a 3D vector as three float elements</summary>
        public void Write(Vector3 data)
        {
            Write(data.x);
            Write(data.y);
            Write(data.z);
        }

        /// <summary>Write an ASCII string element. The string MUST be ASCII-encoded!</summary>
        public void Write(string data)
        {
            foreach (var chr in data)
                Buffer[m_Length++] = (byte) chr;

            var alignedLength = (data.Length + 3) & ~3;
            // if our length was already aligned to 4 bytes, that means we don't have a string terminator yet,
            // so we need to write one, which requires aligning to the next 4-byte mark.
            if (alignedLength == data.Length)
                alignedLength += 4;
            
            for (int i = data.Length; i < alignedLength; i++)
                Buffer[m_Length++] = 0;
        }

        /// <summary>Write an ASCII string element. The string MUST be ASCII-encoded!</summary>
        public void Write(BlobString data)
        {
            var strLength = data.Length;
            System.Buffer.MemoryCopy(data.Handle.Pointer, m_BufferPtr + m_Length, strLength, strLength);
            m_Length += strLength;
            
            var alignedLength = (data.Length + 3) & ~3;
            if (alignedLength == data.Length)
                alignedLength += 4;
            
            for (int i = data.Length; i < alignedLength; i++)
                Buffer[m_Length++] = 0;
        }
        
        /// <summary>Write a blob element</summary>
        /// <param name="bytes">The bytes to copy from</param>
        /// <param name="length">The number of bytes in the blob element</param>
        /// <param name="start">The index in the bytes array to start copying from</param>
        public void Write(byte[] bytes, int length, int start = 0)
        {
            if (start + length > bytes.Length) 
                return;
            
            Write(length);
            System.Buffer.BlockCopy(bytes, start, Buffer, m_Length, length);
            m_Length += length;

            // write any trailing zeros necessary
            var remainder = ((length + 3) & ~3) - length;
            for (int i = 0; i < remainder; i++)
            {
                Buffer[m_Length++] = 0;
            }
        }
        
        /// <summary>Write a 64-bit integer element</summary>
        public void Write(long data)
        {
            var buffer = Buffer;
            buffer[m_Length++] = (byte) (data >> 56);
            buffer[m_Length++] = (byte) (data >> 48);
            buffer[m_Length++] = (byte) (data >> 40);
            buffer[m_Length++] = (byte) (data >> 32);
            buffer[m_Length++] = (byte) (data >> 24);
            buffer[m_Length++] = (byte) (data >> 16);
            buffer[m_Length++] = (byte) (data >>  8);
            buffer[m_Length++] = (byte) (data);
        }
        
        /// <summary>Write a 64-bit floating point element</summary>
        public void Write(double data)
        {
            var buffer = Buffer;
            m_DoubleSwap[0] = data;
            var dsPtr = m_DoubleSwapPtr;
            buffer[m_Length++] = dsPtr[7];
            buffer[m_Length++] = dsPtr[6];
            buffer[m_Length++] = dsPtr[5];
            buffer[m_Length++] = dsPtr[4];
            buffer[m_Length++] = dsPtr[3];
            buffer[m_Length++] = dsPtr[2];
            buffer[m_Length++] = dsPtr[1];
            buffer[m_Length++] = dsPtr[0];
        }
        
        /// <summary>Write a 32-bit RGBA color element</summary>
        public void Write(Color32 data)
        {
            m_Color32Swap[0] = data;
            Buffer[m_Length++] = m_Color32SwapPtr[3];
            Buffer[m_Length++] = m_Color32SwapPtr[2];
            Buffer[m_Length++] = m_Color32SwapPtr[1];
            Buffer[m_Length++] = m_Color32SwapPtr[0];
        }
        
        /// <summary>Write a MIDI message element</summary>
        public void Write(MidiMessage data)
        {
            var midiWritePtr = (MidiMessage*) (m_BufferPtr + m_Length);
            *midiWritePtr = data;
            m_Length += 4;
        }

        /// <summary>Write a 64-bit NTP timestamp element</summary>
        public void Write(NtpTimestamp time)
        {
            time.ToBigEndianBytes((uint*)(m_BufferPtr + m_Length));
            m_Length += 8;
        }

        /// <summary>Write a single ascii character element</summary>
        public void Write(char data)
        {
            // char is written in the last byte of the 4-byte block;
            Buffer[m_Length + 3] = (byte) data;
            m_Length += 4;
        }
        
        /// <summary>Write '#bundle ' at the start of a bundled message</summary>
        public void WriteBundlePrefix()
        {
            const int size = 8;
            // TODO replace with dereferencing the long  version ?
            System.Buffer.BlockCopy(Constant.BundlePrefixBytes, 0, Buffer, m_Length, size);
            m_Length += size;
        }
                
        /// <summary>
        /// Combines Reset(), Write(address), and Write(tags) in a single function to reduce call overhead
        /// </summary>
        /// <param name="address">The OSC address to send to</param>
        /// <param name="tags">4 bytes that represent up to 3 type tags</param>
        public void WriteAddressAndTags(string address, uint tags)
        {
            m_Length = 0;
            foreach (var chr in address)
                Buffer[m_Length++] = (byte) chr;

            var alignedLength = (address.Length + 3) & ~3;
            // if our length was already aligned to 4 bytes, that means we don't have a string terminator yet,
            // so we need to write one, which requires aligning to the next 4-byte mark.
            if (alignedLength == address.Length)
                alignedLength += 4;
            
            for (int i = address.Length; i < alignedLength; i++)
                Buffer[m_Length++] = 0;
            
            // write the 4 bytes for the type tags
            ((uint*)(m_BufferPtr + m_Length))[0] = tags;
            m_Length += 4;
        }

        public void CopyBuffer(byte[] copyTo, int copyOffset = 0)
        {
            System.Buffer.BlockCopy(Buffer, 0, copyTo, copyOffset, Length);
        }

        public void Dispose()
        {
            m_BufferHandle.SafeFree();
            m_Color32SwapHandle.SafeFree();
            m_FloatSwapHandle.SafeFree();
            m_DoubleSwapHandle.SafeFree();
        }
    }
}