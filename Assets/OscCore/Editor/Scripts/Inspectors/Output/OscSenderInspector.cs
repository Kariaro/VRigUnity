using UnityEditor;

namespace OscCore
{
    [CustomEditor(typeof(OscSender))]
    class OscSenderInspector : Editor
    {
        const string k_HelpText = "Handles serializing & sending OSC messages to the given IP address and port.\n" +
                                  "Forwards messages from all property sender components that reference it.";
        
        SerializedProperty m_IpAddressProp;
        SerializedProperty m_PortProp;

        void OnEnable()
        {
            m_IpAddressProp = serializedObject.FindProperty("m_IpAddress");
            m_PortProp = serializedObject.FindProperty("m_Port");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IpAddressProp);
            EditorGUILayout.PropertyField(m_PortProp);

            EditorGUILayout.Space();
            EditorHelp.DrawBox(k_HelpText);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
