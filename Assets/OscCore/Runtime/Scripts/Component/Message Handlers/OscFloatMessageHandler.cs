using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/Float Input")]
    public class OscFloatMessageHandler : OscMessageHandler<float, FloatUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadFloatElement(0);
        }
    }
}
