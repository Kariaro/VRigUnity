using System;
using System.Text;
using UnityEditor;

namespace OscCore
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    static class Constant
    {
        public const byte Comma = (byte) ',';
        public const byte ForwardSlash = (byte) '/';
        
        public static readonly byte[] BundlePrefixBytes;
        public static readonly long BundlePrefixLong;

        static Constant()
        {
            var bundleBytes = Encoding.ASCII.GetBytes("#bundle ");
            bundleBytes[7] = 0;
            BundlePrefixBytes = bundleBytes;
            BundlePrefixLong = BitConverter.ToInt64(bundleBytes, 0);
        }
    }
}