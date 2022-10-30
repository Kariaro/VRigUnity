using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace uOSC
{

[CustomEditor(typeof(uOscClient))]
public class uOscClientEditor : Editor
{
    uOscClient client { get { return target as uOscClient; } }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (EditorUtil.Foldout("Client", true))
        {
            ++EditorGUI.indentLevel;
            DrawClient();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Events", false))
        {
            ++EditorGUI.indentLevel;
            DrawEvents();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Advanced", false))
        {
            ++EditorGUI.indentLevel;
            DrawAdvanced();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Status", false))
        {
            ++EditorGUI.indentLevel;
            DrawStatus();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawClient()
    {
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.address));
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.port));
    }

    void DrawAdvanced()
    {
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.maxQueueSize));
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.dataTransimissionInterval));
    }

    void DrawEvents()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10f, false);
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.onClientStarted));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10f, false);
        EditorUtil.DrawProperty(serializedObject, nameof(uOscClient.onClientStopped));
        EditorGUILayout.EndHorizontal();
    }

    void DrawStatus()
    {
        var skin = GUI.skin.label;
        skin.richText = true;
        var status = client.isRunning ?
            "<color=#5d5>Running</color>" :
            "<color=#888>Stop</color>";
        EditorGUILayout.LabelField("Status", status, skin);
    }
}

}
