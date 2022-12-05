using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    static class OscCoreSettingsIMGUIRegister
    {
        const string k_HelpTooltip = "If enabled, display tutorial & hint messages in the Editor";
        static readonly GUIContent k_HelpContent = new GUIContent("Show Help", k_HelpTooltip);
        
        [SettingsProvider]
        public static SettingsProvider CreateOscCoreSettingsProvider()
        {
            var provider = new SettingsProvider("User/Open Sound Control Core", SettingsScope.User)
            {
                label = "OSC Core",
                guiHandler = (searchContext) =>
                {
                    if (EditorPrefs.HasKey(EditorHelp.PrefKey))
                    {
                        var setting = EditorPrefs.GetBool(EditorHelp.PrefKey);
                        var afterSetting = EditorGUILayout.Toggle(k_HelpContent, setting);
                        if(afterSetting != setting)
                            EditorPrefs.SetBool(EditorHelp.PrefKey, afterSetting);
                    }
                    else
                    {
                        EditorPrefs.SetBool(EditorHelp.PrefKey, true);
                    }
                },

                keywords = new HashSet<string>(new[] { "OSC", "Help" , "Open Sound" })
            };

            return provider;
        }
    }
}
