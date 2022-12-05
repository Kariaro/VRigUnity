using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("OscCore.Tests.Editor")]
[assembly:InternalsVisibleTo("OscCore.Tests.Runtime")]

namespace OscCore
{
    public unsafe class OscParser
    {
        // TODO - make these preferences options
        public const int MaxElementsPerMessage = 32;
        public const int MaxBlobSize = 1024 * 256;

        internal readonly byte[] Buffer;
        internal readonly byte* BufferPtr;
        internal readonly long* BufferLongPtr;

        /// <summary>
        /// Holds all parsed values.  After calling Parse(), this should have data available to read
        /// </summary>
        public readonly OscMessageValues MessageValues;

        /// <summary>Create a new parser.</summary>
        /// <param name="fixedBuffer">The buffer to read messages from.  Must be fixed in memory !</param>
        public OscParser(byte[] fixedBuffer)
        {
            Buffer = fixedBuffer;
            fixed (byte* ptr = fixedBuffer)
            {
                BufferPtr = ptr;
                BufferLongPtr = (long*) ptr;
            }
            MessageValues = new OscMessageValues(Buffer, MaxElementsPerMessage);
        }
        
        /// <summary>
        /// Parse a single non-bundle message that starts at the beginning of the buffer
        /// </summary>
        /// <returns>The unaligned length of the message address</returns>
        public int Parse()
        {
            // address length here doesn't include the null terminator and alignment padding.
            // this is so we can look up the address by only its content bytes.
            var addressLength = FindUnalignedAddressLength();
            if (addressLength < 0)
                return addressLength;    // address didn't start with '/'

            var alignedAddressLength = (addressLength + 3) & ~3;
            // if the null terminator after the string comes at the beginning of a 4-byte block,
            // we need to add 4 bytes of padding
            if (alignedAddressLength == addressLength)
                alignedAddressLength += 4;

            var tagCount = ParseTags(Buffer, alignedAddressLength);
            var offset = alignedAddressLength + (tagCount + 4) & ~3;
            FindOffsets(offset);
            return addressLength;
        }
        
        /// <summary>
        /// Parse a single non-bundle message that starts at the given byte offset from the start of the buffer
        /// </summary>
        /// <returns>The unaligned length of the message address</returns>
        public int Parse(int startingByteOffset)
        {
            // address length here doesn't include the null terminator and alignment padding.
            // this is so we can look up the address by only its content bytes.
            var addressLength = FindUnalignedAddressLength(startingByteOffset);
            if (addressLength < 0)
                return addressLength;    // address didn't start with '/'

            var alignedAddressLength = (addressLength + 3) & ~3;
            // if the null terminator after the string comes at the beginning of a 4-byte block,
            // we need to add 4 bytes of padding
            if (alignedAddressLength == addressLength)
                alignedAddressLength += 4;

            var startPlusAlignedLength = startingByteOffset + alignedAddressLength;
            var tagCount = ParseTags(Buffer, startPlusAlignedLength);
            var offset = startPlusAlignedLength + (tagCount + 4) & ~3;
            FindOffsets(offset);
            return addressLength;
        }

        internal static bool AddressIsValid(string address)
        {
            if (address[0] != '/') return false;

            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case '*':
                    case ',':
                    case '?':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        return false;
                }
            }

            return true;
        }
        
        internal static bool CharacterIsValidInAddress(char c)
        {
            switch (c)
            {
                case ' ':
                case '#':
                case '*':
                case ',':
                case '?':
                case '[':
                case ']':
                case '{':
                case '}':
                    return false;
                default:
                    return true;
            }
        }

        internal static AddressType GetAddressType(string address)
        {
            if (address[0] != '/') return AddressType.Invalid;

            var addressValid = true;
            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case '*':
                    case ',':
                    case '?':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        addressValid = false;
                        break;
                }
            }

            if (addressValid) return AddressType.Address;
            
            // if the address isn't valid, it might be a valid address pattern.
            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case ',':
                        return AddressType.Invalid;
                }
            }

            return AddressType.Pattern;
        }

        internal int ParseTags(byte[] bytes, int start = 0)
        {
            if (bytes[start] != Constant.Comma) return 0;
            
            var tagIndex = start + 1;         // skip the starting ','
            var outIndex = 0;
            var tags = MessageValues.Tags;
            while (true)
            {
                var tag = (TypeTag) bytes[tagIndex];
                if (!tag.IsSupported()) break;
                tags[outIndex] = tag;
                tagIndex++;
                outIndex++; 
            }

            MessageValues.ElementCount = outIndex;
            return outIndex;
        }

        public int FindUnalignedAddressLength()
        {
            if (BufferPtr[0] != Constant.ForwardSlash)
                return -1;
            
            var index = 1;
            do
            {
                index++;
            } 
            while (BufferPtr[index] != byte.MinValue);
            return index;
        }
        
        public int FindUnalignedAddressLength(int offset)
        {
            if (BufferPtr[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;
            do
            {
                index++;
            } 
            while (BufferPtr[index] != byte.MinValue);

            var length = index - offset;
            return length;
        }

        public int GetStringLength(int offset)
        {
            var end = Buffer.Length - offset;
            int index;
            for (index = offset; index < end; index++)
            {
                if (Buffer[index] != 0) break;
            }

            var length = index - offset;
            return (length + 3) & ~3;            // align to 4 bytes
        }

        /// <summary>Find the byte offsets for each element of the message</summary>
        /// <param name="offset">The byte index of the first value</param>
        public void FindOffsets(int offset)
        {
            var tags = MessageValues.Tags;
            var offsets = MessageValues.Offsets;
            for (int i = 0; i < MessageValues.ElementCount; i++)
            {
                offsets[i] = offset;
                switch (tags[i])
                {
                    // false, true, nil, infinitum & array[] tags add 0 to the offset
                    case TypeTag.Int32:
                    case TypeTag.Float32:
                    case TypeTag.Color32:    
                    case TypeTag.AsciiChar32:
                    case TypeTag.MIDI:
                        offset += 4; 
                        break;
                    case TypeTag.Float64:
                    case TypeTag.Int64:
                    case TypeTag.TimeTag:
                        offset += 8;
                        break;
                    case TypeTag.String:
                    case TypeTag.AltTypeString:
                        offset += GetStringLength(offset);
                        break;
                    case TypeTag.Blob:
                        // read the int that specifies the size of the blob
                        offset += 4 + *(int*)(BufferPtr + offset);
                        break;
                }
            }
        }

        /// <summary>
        /// Test if '#bundle ' is present at a given index in the buffer 
        /// </summary>
        /// <param name="index">The index in the buffer to test</param>
        /// <returns>True if present, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBundleTagAtIndex(int index)
        {
            return *((long*) BufferPtr + index) == Constant.BundlePrefixLong;
        }
        
        public static int FindArrayLength(byte[] bytes, int offset = 0)
        {
            if ((TypeTag) bytes[offset] != TypeTag.ArrayStart)
                return -1;
            
            var index = offset + 1;
            while (bytes[index] != (byte) TypeTag.ArrayEnd)
                index++;

            return index - offset;
        }
    }
}

