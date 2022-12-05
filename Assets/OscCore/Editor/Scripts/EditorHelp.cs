using UnityEditor;

namespace OscCore
{
    static class EditorHelp
    {
        public const string PrefKey = "OscCore_ShowEditorHelp";

        public static bool Show => EditorPrefs.GetBool(PrefKey, true);

        public static void DrawBox(string text, MessageType type = MessageType.Info)
        {
            if(EditorPrefs.GetBool(PrefKey, true))
                EditorGUILayout.HelpBox(text, type);
        }
    }
}
