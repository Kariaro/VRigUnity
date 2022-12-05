using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/Long Input")]
    public class OscInt64MessageHandler : OscMessageHandler<long, LongUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadInt64Element(0);
        }
    }
}
