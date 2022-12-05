using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlobHandles;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    public class MonitorWindow : EditorWindow
    {
        const int showLinesCount = 32;
        
        static readonly StringBuilder k_Builder = new StringBuilder();
        
        OscServer m_Server;
        
        readonly Queue<string> m_LogMessages = new Queue<string>(showLinesCount);
        readonly List<string> m_ToQueue = new List<string>(16);
        readonly List<string> m_ToQueueAlt = new List<string>(16);
        bool m_UseAlt;
        List<string> m_ActiveQueueBuffer;

        string m_ServerLabel = "";
        
        bool m_NeedsRepaint;
        bool m_ShowNoServerWarning;
        int m_PreviousServerCount;

        bool m_PreviouslyPlaying;
        
        void OnEnable()
        {
            m_PreviousServerCount = 0;
            m_ActiveQueueBuffer = m_ToQueue;
            if (OscServer.PortToServer.Count == 0)
                m_ShowNoServerWarning = true;
            
            HandleServerChanges();
        }

        void OnDisable()
        {
            m_Server?.RemoveMonitorCallback(Monitor);
            m_PreviousServerCount = 0;
        }

        void HandleServerChanges()
        {
            if (OscServer.PortToServer.Count > m_PreviousServerCount)
            {
                m_Server = OscServer.PortToServer.First().Value;
                m_Server.AddMonitorCallback(Monitor);
                m_ServerLabel = $"Messages being received on port {m_Server.Port}";
                m_ShowNoServerWarning = false;

                if (!Application.isPlaying)
                    EditorApplication.update += Update;
            }
            else if (OscServer.PortToServer.Count < m_PreviousServerCount)
            {
                m_Server?.RemoveMonitorCallback(Monitor);
                m_Server = null;
                m_ServerLabel = null;
                m_ShowNoServerWarning = true;
                m_PreviousServerCount = OscServer.PortToServer.Count;
                
                if (!Application.isPlaying)
                    EditorApplication.update -= Update;
            }
            
            m_PreviousServerCount = OscServer.PortToServer.Count;
        }

        void HandleModeChange()
        {
            if (EditorApplication.isPlaying != m_PreviouslyPlaying)
            {
                m_PreviousServerCount = 0;
                lock (m_LogMessages)
                {
                    m_LogMessages.Clear();
                    m_ToQueueAlt.Clear();
                    m_ToQueue.Clear();
                }
            }

            m_PreviouslyPlaying = EditorApplication.isPlaying;
        }

        void Update()
        {
            if (Time.frameCount % 2 != 0) return;
            
            HandleModeChange();
            HandleServerChanges();

            lock (m_LogMessages)
            {
                if (m_LogMessages.Count == 0 && m_ToQueue.Count == 0 && m_ToQueueAlt.Count == 0) 
                    return;
                
                var useAlt = m_UseAlt;
                m_UseAlt = !m_UseAlt;
                if (useAlt)
                {
                    try
                    {
                        foreach (var msg in m_ToQueueAlt)
                        {
                            m_LogMessages.Enqueue(msg);
                        }
                    }
                    catch (InvalidOperationException) { }

                    m_ToQueueAlt.Clear();
                }
                else
                {
                    try
                    {
                        foreach (var msg in m_ToQueue)
                        {
                            m_LogMessages.Enqueue(msg);
                        }

                        m_ToQueue.Clear();
                    }
                    catch (InvalidOperationException) { }
                }

                while (m_LogMessages.Count > showLinesCount)
                {
                    m_LogMessages.Dequeue();
                }
            }
            
            if(m_NeedsRepaint) Repaint();
        }

        public void OnGUI()
        {
            const string noServerWarning = "No OSC Servers are currently active, so no messages can be received";
            var label = m_ShowNoServerWarning ? noServerWarning : m_ServerLabel;
            EditorGUILayout.HelpBox(label, MessageType.Info);

            lock (m_LogMessages)
            {
                foreach (var line in m_LogMessages)
                {
                    EditorGUILayout.LabelField(line);
                }
            }
        }

        void Monitor(BlobString address, OscMessageValues values)
        {
            if(m_UseAlt)
                m_ToQueueAlt.Add(MessageToString(address.ToString(), values));
            else
                m_ToQueue.Add(MessageToString(address.ToString(), values));

            m_NeedsRepaint = true;
        }
        
        static string MessageToString(string address, OscMessageValues values)
        {
            k_Builder.Clear();
            k_Builder.Append(address);
            const string divider = "    ,";
            k_Builder.Append(divider);
            values.ForEachElement((i, type) => { k_Builder.Append((char)type); });
            k_Builder.Append("    ");

            var lastIndex = values.ElementCount - 1;
            values.ForEachElement((i, type) =>
            {
                var elementText = values.ReadStringElement(i);
                k_Builder.Append(elementText);
                if(i != lastIndex) k_Builder.Append(' ');
            });

            return k_Builder.ToString();
        }

        [MenuItem("Window/OscCore/Monitor")]
        static void InitWindow()
        {
            ((MonitorWindow) GetWindow(typeof(MonitorWindow)))?.Show();
        }
    }
}


