using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlobHandles
{
    /// <summary>
    /// Wraps an arbitrary chunk of bytes in memory, so it can be used as a hash key
    /// and compared against other instances of the same set of bytes 
    /// </summary>
    public unsafe struct BlobHandle : IEquatable<BlobHandle>
    {
        /// <summary>A pointer to the start of the blob</summary>
        public readonly byte* Pointer;
        /// <summary>The number of bytes in the blob</summary>
        public readonly int Length;

        public BlobHandle(byte* pointer, int length)
        {
            Pointer = pointer;
            Length = length;
        }
        
        public BlobHandle(IntPtr pointer, int length)
        {
            Pointer = (byte*) pointer;
            Length = length;
        }
        
        /// <summary>
        /// Get a blob handle for a byte array. The byte array should have its address pinned to work safely!
        /// </summary>
        /// <param name="bytes">The bytes to get a handle to</param>
        public BlobHandle(byte[] bytes)
        {
            fixed (byte* ptr = bytes)
            {
                Pointer = ptr;
                Length = bytes.Length;
            }
        }

        /// <summary>
        /// Get a blob handle for part of a byte array. The byte array should have its address pinned to work safely!
        /// </summary>
        /// <param name="bytes">The bytes to get a handle to</param>
        /// <param name="length">The number of bytes to include. Not bounds checked</param>
        public BlobHandle(byte[] bytes, int length)
        {
            fixed (byte* ptr = bytes)
            {
                Pointer = ptr;
                Length = length;
            }
        }

        /// <summary>
        /// Get a blob handle for a slice of a byte array. The byte array should have its address pinned to work safely!
        /// </summary>
        /// <param name="bytes">The bytes to get a handle to</param>
        /// <param name="length">The number of bytes to include. Not bounds checked</param>
        /// <param name="offset">The byte array index to start the blob at</param>
        public BlobHandle(byte[] bytes, int length, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                Pointer = ptr;
                Length = length;
            }
        }
        
        public override string ToString()
        {
            return $"{Length.ToString()} bytes @ {new IntPtr(Pointer).ToString()}";
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                return Length * 397 ^ Pointer[Length - 1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BlobHandle other)
        {
            return Length == other.Length && 
                   MemoryCompare(Pointer, other.Pointer, (UIntPtr) Length) == 0;
        }
        
        public override bool Equals(object obj)
        {
            return obj is BlobHandle other && Equals(other);
        }

        public static bool operator ==(BlobHandle left, BlobHandle right)
        {
            return left.Length == right.Length && 
                   MemoryCompare(left.Pointer, right.Pointer, (UIntPtr) left.Length) == 0;
        }

        public static bool operator !=(BlobHandle left, BlobHandle right)
        {
            return left.Length != right.Length || 
                   MemoryCompare(left.Pointer, right.Pointer, (UIntPtr) left.Length) != 0;
        }
                
        // comparing bytes using memcmp has shown to be several times faster than any other method i've found
        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        static extern int MemoryCompare(void* ptr1, void* ptr2, UIntPtr count);

    }
}

