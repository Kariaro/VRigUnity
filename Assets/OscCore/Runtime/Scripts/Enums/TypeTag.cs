using System.Runtime.CompilerServices;

namespace OscCore
{
    /// <summary>
    /// type tags from http://opensoundcontrol.org/spec-1_0 
    /// </summary>
    public enum TypeTag : byte
    {
        False = 70,                     // F, non-standard
        Infinitum = 73,                 // I, non-standard
        Nil = 78,                       // N, non-standard
        AltTypeString = 84,             // S, non-standard
        True = 85,                      // T, non-standard
        ArrayStart = 91,                // [, non-standard
        ArrayEnd = 93,                  // ], non-standard
        Blob = 98,                      // b, STANDARD
        AsciiChar32 = 99,               // c, non-standard
        Float64 = 100,                  // d, non-standard
        Float32 = 102,                  // f, STANDARD
        Int64 = 104,                    // h, non-standard
        Int32 = 105,                    // i, STANDARD
        MIDI = 109,                     // m, non-standard
        Color32 = 114,                  // r, non-standard
        String = 115,                   // s, STANDARD
        TimeTag = 116                   // t, non-standard
    }

    public static class TypeTagMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSupported(this TypeTag tag)
        {
            switch (tag)
            {
                case TypeTag.False:
                case TypeTag.Infinitum:
                case TypeTag.Nil:
                case TypeTag.AltTypeString:
                case TypeTag.True:
                case TypeTag.Blob:
                case TypeTag.AsciiChar32:
                case TypeTag.Float64:
                case TypeTag.Float32:
                case TypeTag.Int64: 
                case TypeTag.Int32: 
                case TypeTag.MIDI: 
                case TypeTag.Color32: 
                case TypeTag.String: 
                case TypeTag.TimeTag: 
                case TypeTag.ArrayStart:
                case TypeTag.ArrayEnd: 
                    return true;
                default: 
                    return false;
            }
        }
    }
}

