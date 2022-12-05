using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BlobHandles
{
    public static unsafe class BlobHandleDictionaryMethods
    {
        /// <summary>
        /// Try to find the value associated with a given chunk of bytes
        /// </summary>
        /// <param name="self">The dictionary to look in</param>
        /// <param name="ptr">Pointer to the start of the bytes</param>
        /// <param name="length">The number of bytes to read</param>
        /// <param name="value">The output value</param>
        /// <typeparam name="T">The dictionary value type</typeparam>
        /// <returns>True if the value was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueFromBytes<T>(this Dictionary<BlobHandle, T> self, 
            byte* ptr, int length, out T value)
        {
            return self.TryGetValue(new BlobHandle(ptr, length), out value);
        }
        
        /// <summary>
        /// Try to find the value associated with a given chunk of bytes
        /// </summary>
        /// <param name="self">The dictionary to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <param name="value">The output value</param>
        /// <typeparam name="T">The dictionary value type</typeparam>
        /// <returns>True if the value was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueFromBytes<T>(this Dictionary<BlobHandle, T> self, 
            byte[] bytes, out T value)
        {
            return self.TryGetValue(new BlobHandle(bytes), out value);
        }

        /// <summary>
        /// Try to find the value associated with a given chunk of bytes
        /// </summary>
        /// <param name="self">The dictionary to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <param name="length">The number of bytes to read</param>
        /// <param name="value">The output value</param>
        /// <typeparam name="T">The dictionary value type</typeparam>
        /// <returns>True if the value was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueFromBytes<T>(this Dictionary<BlobHandle, T> self, 
            byte[] bytes, int length, out T value)
        {
            return self.TryGetValue(new BlobHandle(bytes, length), out value);
        }

        /// <summary>
        /// Try to find the value associated with a given chunk of bytes.
        /// No bounds checking is performed, so be certain that offset + length is within the byte array.
        /// </summary>
        /// <param name="self">The dictionary to look in</param>
        /// <param name="bytes">The byte array to read from</param>
        /// <param name="length">The number of bytes to read</param>
        /// <param name="offset">The index in the byte array to start reading at</param>
        /// <param name="value">The output value</param>
        /// <typeparam name="T">The dictionary value type</typeparam>
        /// <returns>True if the value was found, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueFromBytes<T>(this Dictionary<BlobHandle, T> self, 
            byte[] bytes, int length, int offset, out T value)
        {
            return self.TryGetValue(new BlobHandle(bytes, length, offset), out value);
        }
    }
}