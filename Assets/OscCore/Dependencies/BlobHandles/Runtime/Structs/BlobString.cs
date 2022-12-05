using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BlobHandles
{
    /// <summary>
    /// Represents a string as a fixed blob of bytes
    /// </summary>
    public struct BlobString : IDisposable, IEquatable<BlobString>
    {
        /// <summary>
        /// The encoding used to convert to and from strings.
        /// WARNING - Changing this after strings have been encoded will probably lead to errors!
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.ASCII;
        
        // Stores all of the bytes that represent this string
        readonly NativeArray<byte> Bytes; 
        
        public readonly BlobHandle Handle;

        public int Length => Bytes.Length;
        
        public unsafe BlobString(string source, Allocator allocator = Allocator.Persistent)
        {
            var byteCount = Encoding.GetByteCount(source);
            Bytes = new NativeArray<byte>(byteCount, allocator);
            var nativeBytesPtr = (byte*) Bytes.GetUnsafePtr();
            
            // write encoded string bytes directly to unmanaged memory
            fixed (char* strPtr = source)
            {
                Encoding.GetBytes(strPtr, source.Length, nativeBytesPtr, byteCount);
                Handle = new BlobHandle(nativeBytesPtr, byteCount);
            }
        }
        
        public unsafe BlobString(byte* sourcePtr, int length)
        {
            Handle = new BlobHandle(sourcePtr, length);
            Bytes = default;
        }
                
        public override unsafe string ToString()
        {
            return Encoding.GetString(Handle.Pointer, Handle.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BlobString other)
        {
            return Handle.Equals(other.Handle);
        }
        
        public override bool Equals(object obj)
        {
            return obj is BlobString other && Handle.Equals(other.Handle);
        }

        public static bool operator ==(BlobString l, BlobString r)
        {
            return l.Handle == r.Handle;
        }

        public static bool operator !=(BlobString l, BlobString r)
        {
            return l.Handle != r.Handle;
        }
        
        public void Dispose()
        {
            if(Bytes.IsCreated) Bytes.Dispose();
        }
    }
}
