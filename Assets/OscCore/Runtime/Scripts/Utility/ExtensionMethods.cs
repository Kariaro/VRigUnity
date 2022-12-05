using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OscCore
{
    static class ExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align4(this int self)
        {
            return (self + 3) & ~3;
        }

        internal static void SafeFree(this GCHandle handle)
        {
            if(handle.IsAllocated) handle.Free();
        }
        
        internal static int ClampPort(this int port)
        {
            if (port < 1024) port = 1024;
            if (port >= 65535) port = 65535;
            return port;
        }
    }
}