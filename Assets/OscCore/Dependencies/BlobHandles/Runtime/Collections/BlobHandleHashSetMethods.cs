using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BlobHandles
{
    public static unsafe class BlobHandleHashSetMethods
    {
        /// <summary>Determines whether the HashSet contains the specified sequence of bytes</summary>
        /// <param name="self">The HashSet to look in</param>
        /// <param name="ptr">Pointer to the start of the bytes</param>
        /// <param name="length">The number of bytes to read</param>
        /// <returns>True if the key was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBlob<T>(this HashSet<BlobHandle> self, byte* ptr, int length)
        {
            return self.Contains(new BlobHandle(ptr, length));
        }
        
        /// <summary>Determines whether the HashSet contains the specified sequence of bytes</summary>
        /// <param name="self">The HashSet to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <returns>True if the key was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBlob<T>(this HashSet<BlobHandle> self, byte[] bytes)
        {
            return self.Contains(new BlobHandle(bytes));
        }

        /// <summary>
        /// Determines whether the HashSet contains the specified sequence of bytes
        /// No bounds checking is performed, so be certain that length is less than or equal to bytes.Length
        /// </summary>
        /// <param name="self">The HashSet to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <param name="length">The number of bytes to read</param>
        /// <returns>True if the key was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBlob<T>(this HashSet<BlobHandle> self, byte[] bytes, int length)
        {
            return self.Contains(new BlobHandle(bytes, length));
        }

        /// <summary>
        /// Determines whether the HashSet contains the specified sequence of bytes
        /// No bounds checking is performed, so be certain that offset + length is within end of the byte array.
        /// </summary>
        /// <param name="self">The HashSet to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <param name="length">The number of bytes to read</param>
        /// <param name="offset">The index in the byte array to start reading at</param>
        /// <returns>True if the key was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBlob(this HashSet<BlobHandle> self, byte[] bytes, int length, int offset)
        {
            return self.Contains(new BlobHandle(bytes, length, offset));
        }
    }
}