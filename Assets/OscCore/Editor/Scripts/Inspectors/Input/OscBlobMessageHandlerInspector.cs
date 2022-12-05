using UnityEditor;

namespace OscCore
{
    [CustomEditor(typeof(OscBlobMessageHandler))]
    class OscBlobMessageHandlerInspector : Editor
    {
        SerializedProperty m_ReceiverProp;
        SerializedProperty m_AddressProp;
        SerializedProperty m_OnReceivedProp;

        void OnEnable()
        {
            m_ReceiverProp = serializedObject.FindProperty("m_Receiver");
            m_AddressProp = serializedObject.FindProperty("m_Address");
            m_OnReceivedProp = serializedObject.FindProperty("OnMessageReceived");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ReceiverProp);
            EditorGUILayout.PropertyField(m_AddressProp);
            EditorGUILayout.Space();
            if (m_OnReceivedProp != null)
                EditorGUILayout.PropertyField(m_OnReceivedProp);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
