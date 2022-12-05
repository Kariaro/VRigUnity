using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/Vector3 Input")]
    public class OscVector3MessageHandler : OscMessageHandler<Vector3, Vector3UnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            var x = values.ReadFloatElement(0);
            var y = values.ReadFloatElement(1);
            var z = values.ReadFloatElement(2);
            m_Value = new Vector3(x, y, z);
        }
    }
}
