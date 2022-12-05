using System;
using UnityEngine;
using UnityEngine.Events;

namespace OscCore
{
    [Serializable] public class BoolUnityEvent : UnityEvent<bool> { }
    [Serializable] public class IntUnityEvent : UnityEvent<int> { }
    [Serializable] public class LongUnityEvent : UnityEvent<long> { }
    [Serializable] public class FloatUnityEvent : UnityEvent<float> { }
    [Serializable] public class DoubleUnityEvent : UnityEvent<double> { }
    [Serializable] public class StringUnityEvent : UnityEvent<string> { }
    [Serializable] public class ColorUnityEvent : UnityEvent<Color> { }
    [Serializable] public class Vector3UnityEvent : UnityEvent<Vector3> { }
    [Serializable] public class BlobUnityEvent : UnityEvent<byte[], int> { }
}