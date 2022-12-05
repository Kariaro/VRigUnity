using UnityEngine;

namespace OscCore
{
    [ExecuteInEditMode]
    [AddComponentMenu("OSC/Input/Blob Input")]
    public class OscBlobMessageHandler : MessageHandlerBase
    {
        public BlobUnityEvent OnMessageReceived;
        
        protected byte[] m_Buffer = new byte[128];

        public byte[] Buffer => m_Buffer;
        public int LastReceivedBlobLength { get; private set; }

        protected override void ValueRead(OscMessageValues values)
        {
            LastReceivedBlobLength = values.ReadBlobElement(0, ref m_Buffer);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Buffer, LastReceivedBlobLength);
        }
    }
}
