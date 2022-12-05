using System;
using System.Reflection;
using UnityEngine;

namespace OscCore
{
    [ExecuteInEditMode]
    [AddComponentMenu("OSC/Property Output", int.MaxValue)]
    public class PropertyOutput : MonoBehaviour
    {
#pragma warning disable 649
        [Tooltip("Component that handles sending outgoing OSC messages")]
        [SerializeField] OscSender m_Sender;
        
        [Tooltip("The OSC address to send to at the destination")]
        [SerializeField] string m_Address = "";
        
        [Tooltip("The object host of the component where the property lives")]
        [SerializeField] GameObject m_Object;
        [SerializeField] Component m_SourceComponent;
        
        [SerializeField] bool m_MemberIsProperty;
        [SerializeField] string m_PropertyName;
        [SerializeField] string m_PropertyTypeName;
        
        [SerializeField] Vector2ElementFilter m_SendVector2Elements;
        [SerializeField] Vector3ElementFilter m_SendVector3Elements;
#pragma warning restore 649

        bool m_PreviousBooleanValue;
        int m_PreviousIntValue;
        long m_PreviousLongValue;
        double m_PreviousDoubleValue;
        float m_PreviousSingleValue;
        string m_PreviousStringValue;
        Color m_PreviousColorValue;
        Vector2 m_PreviousVec2Value;
        Vector3 m_PreviousVec3Value;

        bool m_HasSender;

        MemberInfo m_MemberInfo;
        PropertyInfo m_Property;
        FieldInfo m_Field;
        
        /// <summary>
        /// The OscCore component that handles serializing and sending messages. Cannot be null
        /// </summary>
        public OscSender Sender
        {
            get => m_Sender;
            set => m_Sender = value == null ? m_Sender : value;
        }

        /// <summary>
        /// The Unity component that has the property to send.
        /// Must be a non-null type that has the current property
        /// </summary>
        public Component SourceComponent
        {
            get => m_SourceComponent;
            set => m_SourceComponent = value == null ? m_SourceComponent : value;
        }
        
        /// <summary>
        /// The property to send the value of.  Must be a property found on the current SourceComponent.
        /// Will be null if the member being sent is a Field.
        /// </summary>
        public PropertyInfo Property
        {
            get => m_Property;
            set
            {
                m_MemberInfo = value;
                m_Property = value;
                m_Field = null;
                m_MemberIsProperty = true;
            }
        }

        /// <summary>
        /// The Field to send the value of.  Must be a public field found on the current SourceComponent.
        /// Will be null if the member being sent is a Property.
        /// </summary>
        public FieldInfo Field
        {
            get => m_Field;
            set
            {
                m_MemberInfo = value;
                m_Field = value;
                m_Property = null;
                m_MemberIsProperty = false;
            }
        }

        void OnEnable()
        {
            if (m_Object == null) m_Object = gameObject;
            m_HasSender = m_Sender != null;
            SetPropertyFromSerialized();
        }

        void OnValidate()
        {
            Utils.ValidateAddress(ref m_Address);
            if (m_Sender == null) m_Sender = gameObject.GetComponentInParent<OscSender>();
            m_HasSender = m_Sender != null;
        }

        void Update()
        {
            if (m_MemberInfo == null || !m_HasSender || m_Sender.Client == null) 
                return;

            var value = m_MemberIsProperty ? m_Property.GetValue(m_SourceComponent) : m_Field.GetValue(m_SourceComponent);
            if (value == null)
                return;
            
            switch (m_PropertyTypeName)
            {
                case "Byte":
                case "SByte": 
                case "Int16":
                case "UInt16":    
                case "Int32":
                    if(ValueChanged(ref m_PreviousIntValue, value, out var intVal))
                        m_Sender.Client.Send(m_Address, intVal);
                    break;
                case "Int64":
                    if(ValueChanged(ref m_PreviousLongValue, value, out var longVal))
                        m_Sender.Client.Send(m_Address, longVal);
                    break;
                case "Single":
                    if(ValueChanged(ref m_PreviousSingleValue, value, out var floatVal))
                        m_Sender.Client.Send(m_Address, floatVal);
                    break;
                case "Double":
                    if(ValueChanged(ref m_PreviousDoubleValue, value, out var doubleVal))
                        m_Sender.Client.Send(m_Address, doubleVal);
                    break;
                case "String":
                    if(ValueChanged(ref m_PreviousStringValue, value, out var stringVal))
                        m_Sender.Client.Send(m_Address, stringVal);
                    break;
                case "Color":
                case "Color32":
                    if(ValueChanged(ref m_PreviousColorValue, value, out var colorVal))
                        m_Sender.Client.Send(m_Address, colorVal);
                    break;
                case "Vector2":
                    SendVector2(value);
                    break;
                case "Vector3":
                    SendVector3(value);
                    break;
                case "Boolean":
                    if(ValueChanged(ref m_PreviousBooleanValue, value, out var boolVal))
                        m_Sender.Client.Send(m_Address, boolVal);
                    break;
            }
        }
        
        void SendVector2(object obj)
        {
            var vec = (Vector2) obj;
            switch (m_SendVector2Elements)
            {
                case Vector2ElementFilter.XY:
                    if (!m_PreviousVec2Value.Equals(vec))
                    {
                        m_PreviousVec2Value = vec;
                        m_Sender.Client.Send(m_Address, vec);
                    }
                    break;
                case Vector2ElementFilter.X:
                    if (!m_PreviousSingleValue.Equals(vec.x))
                    {
                        m_PreviousSingleValue = vec.x;
                        m_Sender.Client.Send(m_Address, vec.x);
                    }
                    break;
                case Vector2ElementFilter.Y:
                    if (!m_PreviousSingleValue.Equals(vec.y))
                    {
                        m_PreviousSingleValue = vec.y;
                        m_Sender.Client.Send(m_Address, vec.y);
                    }
                    break;
            }
        }

        void SendVector3(object value)
        {
            var vec = (Vector3) value;

            switch (m_SendVector3Elements)
            {
                case Vector3ElementFilter.XYZ:
                    if (!m_PreviousVec3Value.Equals(vec))
                    {
                        m_PreviousVec3Value = vec;
                        m_Sender.Client.Send(m_Address, vec);
                    }
                    break;
                case Vector3ElementFilter.X:
                    if (!m_PreviousSingleValue.Equals(vec.x))
                    {
                        m_PreviousSingleValue = vec.x;
                        m_Sender.Client.Send(m_Address, vec.x);
                    }
                    break;
                case Vector3ElementFilter.Y:
                    if (!m_PreviousSingleValue.Equals(vec.y))
                    {
                        m_PreviousSingleValue = vec.y;
                        m_Sender.Client.Send(m_Address, vec.y);
                    }
                    break;
                case Vector3ElementFilter.Z:
                    if (!m_PreviousSingleValue.Equals(vec.z))
                    {
                        m_PreviousSingleValue = vec.z;
                        m_Sender.Client.Send(m_Address, vec.z);
                    }
                    break;
                case Vector3ElementFilter.XY:
                    var xy = new Vector2(vec.x, vec.y);
                    if (!m_PreviousVec2Value.Equals(xy))
                    {
                        m_PreviousVec2Value = xy;
                        m_Sender.Client.Send(m_Address, xy);
                    }
                    break;
                case Vector3ElementFilter.XZ:
                    var xz = new Vector2(vec.x, vec.z);
                    if (!m_PreviousVec2Value.Equals(xz))
                    {
                        m_PreviousVec2Value = xz;
                        m_Sender.Client.Send(m_Address, xz);
                    }
                    break;
                case Vector3ElementFilter.YZ:
                    var yz = new Vector2(vec.y, vec.z);
                    if (!m_PreviousVec2Value.Equals(yz))
                    {
                        m_PreviousVec2Value = yz;
                        m_Sender.Client.Send(m_Address, yz);
                    }
                    break;
            }
        }

        static bool ValueChanged<T>(ref T previousValue, object value, out T castValue) where T: IEquatable<T>
        {
            castValue = (T) value;
            if (!castValue.Equals(previousValue))
            {
                previousValue = castValue;
                return true;
            }

            return false;
        }

        internal Component[] GetObjectComponents()
        {
            return m_Object == null ? null : m_Object.GetComponents<Component>();
        }

        internal void SetPropertyFromSerialized()
        {
            if (m_SourceComponent == null) 
                return;
            
            var type = m_SourceComponent.GetType();

            if(m_MemberIsProperty)
                Property = type.GetProperty(m_PropertyName);
            else
                Field = type.GetField(m_PropertyName);
        }
    }
}
