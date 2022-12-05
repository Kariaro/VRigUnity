using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    [CustomEditor(typeof(OscReceiver))]
    class OscReceiverInspector : Editor
    {
        const string k_HelpText = "Handles receiving & parsing OSC messages on the given port.\n" +
                                  "Forwards messages to all event handler components that reference it.";
        
        static readonly List<string> k_SortedAddresses = new List<string>();

        OscReceiver m_Target;
        SerializedProperty m_PortProp;
        
        bool m_ShowAddressFoldout;
        bool m_PreviouslyRunning;
        
        void OnEnable()
        {
            m_Target = (OscReceiver) target;
            m_PortProp = serializedObject.FindProperty("m_Port");
            
            SortAddresses();
        }

        public override void OnInspectorGUI()
        {
            var running = m_Target != null && m_Target.Running;
            if (running != m_PreviouslyRunning)
                SortAddresses();
            
            m_PreviouslyRunning = running;
            
            EditorGUI.BeginDisabledGroup(running && Application.IsPlaying(this));
            EditorGUILayout.PropertyField(m_PortProp);
            EditorGUI.EndDisabledGroup();

            using (new EditorGUI.DisabledScope(!running))
            {
                var count = CountHandlers();
                var prefix = m_ShowAddressFoldout ? "Hide" : "Show";
                m_ShowAddressFoldout = EditorGUILayout.Foldout(m_ShowAddressFoldout, $"{prefix} {count} Listening Addresses", true);
            
                if (m_ShowAddressFoldout)
                {
                    foreach (var addr in k_SortedAddresses)
                        EditorGUILayout.LabelField(addr, EditorStyles.miniBoldLabel);
                }
            }
           
            serializedObject.ApplyModifiedProperties();
            
            if (EditorHelp.Show)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(k_HelpText, MessageType.Info);
            }
        }

        int CountHandlers()
        {
            return m_Target == null || m_Target.Server == null ? 0 : m_Target.Server.CountHandlers();
        }

        void SortAddresses()
        {
            if (m_Target == null || m_Target.Server == null) 
                return;
            
            k_SortedAddresses.Clear();
            k_SortedAddresses.AddRange(m_Target.Server.AddressSpace.Addresses);
            k_SortedAddresses.Sort();
        }
    }
}
