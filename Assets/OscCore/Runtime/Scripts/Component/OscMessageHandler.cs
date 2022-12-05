using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace OscCore
{
    [ExecuteInEditMode]
    public abstract class MessageHandlerBase : MonoBehaviour
    {
        [Tooltip("The receiver to handle messages from")]
        [FormerlySerializedAs("Receiver")]
        [SerializeField] 
        protected OscReceiver m_Receiver;
        public OscReceiver Receiver => m_Receiver;
    
        [Tooltip("The OSC address to associate with this event.  Must start with /")]
        [FormerlySerializedAs("Address")]
        [SerializeField] 
        protected string m_Address = "/";
        public string Address => m_Address;
        
        protected OscActionPair m_ActionPair;
        protected bool m_Registered;
        
        void OnEnable()
        {
            if (m_Receiver == null)
                m_Receiver = GetComponentInParent<OscReceiver>();
            
            if (m_Registered || string.IsNullOrEmpty(Address))
                return;

            if (m_Receiver != null && m_Receiver.Server != null)
            {
                m_ActionPair = new OscActionPair(ValueRead, InvokeEvent);
                Receiver.Server.TryAddMethodPair(Address, m_ActionPair);
                m_Registered = true;
            }
        }

        void OnDisable()
        {
            m_Registered = false;
            if (m_Receiver != null)
                m_Receiver.Server?.RemoveMethodPair(Address, m_ActionPair);
        }
        
        void OnValidate()
        {
            Utils.ValidateAddress(ref m_Address);
        }

        protected abstract void InvokeEvent();
    
        protected abstract void ValueRead(OscMessageValues values);

        // Empty update method here so the component gets an enable checkbox
        protected virtual void Update() { }
    }
    
    [ExecuteInEditMode]
    public abstract class OscMessageHandler<T, TUnityEvent> : MessageHandlerBase
        where TUnityEvent : UnityEvent<T>
    {
        [FormerlySerializedAs("Handler")]
        public TUnityEvent OnMessageReceived;
        
        protected T m_Value;
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }

}

