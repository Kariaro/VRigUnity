using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/Boolean Input")]
    public class OscBooleanMessageHandler : OscMessageHandler<bool, BoolUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadBooleanElement(0);
        }
    }
}
