using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    [CustomEditor(typeof(PropertyOutput), true)]
    class PropertyOutputInspector : Editor
    {
        class MemberInfoComparer : IComparer<MemberInfo>
        {
            public int Compare(MemberInfo x, MemberInfo y)
            {
                return string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
            }
        }

        static readonly MemberInfoComparer k_MemberComparer = new MemberInfoComparer();
        
        static readonly GUIContent k_EmptyContent = new GUIContent();
        static readonly GUIContent k_PropTypeContent = new GUIContent("Type", "The type of the selected property");
        static readonly GUIContent k_ComponentContent = new GUIContent("Component", 
            "The component on the game object that has the property you want");
        static readonly GUIContent k_PropertyContent = new GUIContent("Property", 
            "The component property to get the value of");

        static readonly HashSet<string> k_SupportedTypes = new HashSet<string>()
        {
            "System.SByte", "System.Byte", "System.Int16", "System.UInt16", "System.Int32", "System.Int64",
            "System.Single", "System.Double", "System.String", "System.Boolean",
            "UnityEngine.Vector2", "UnityEngine.Vector3", "UnityEngine.Color", "UnityEngine.Color32"
        };
        
        static GUIContent s_SendVec2ElementsContent;
        static GUIContent s_SendVec3ElementsContent;
        
        SerializedProperty m_AddressProp;
        SerializedProperty m_SenderProp;
        SerializedProperty m_ObjectProp;
        SerializedProperty m_PropertyNameProp;
        SerializedProperty m_PropertyTypeNameProp;
        SerializedProperty m_SourceComponentProp;
        SerializedProperty m_SendVector2ElementsProp;
        SerializedProperty m_SendVector3ElementsProp;

        Component[] m_CachedComponents;
        string[] m_CachedComponentNames;
        string m_PreviousComponentName;
        int m_ComponentIndex;

        MemberInfo[] m_PropertiesAndFields;
        string[] m_PropertyNames;
        int m_PropertyIndex;
        
        bool m_DrawVector2Filter;
        bool m_DrawVector3Filter;
        Vector3ElementFilter m_PreviousVec3FilterEnumValue;
        Vector2ElementFilter m_PreviousVec2FilterEnumValue;
        string m_FilterHelpLabel;
        
        PropertyOutput m_Target;
        bool m_ObjectPreviouslyNotNull;
        
        void OnEnable()
        {
            m_Target = target as PropertyOutput;
            m_AddressProp = serializedObject.FindProperty("m_Address");
            m_SenderProp = serializedObject.FindProperty("m_Sender");
            m_ObjectProp = serializedObject.FindProperty("m_Object");
            m_SourceComponentProp = serializedObject.FindProperty("m_SourceComponent");
            m_PropertyNameProp = serializedObject.FindProperty("m_PropertyName");
            m_PropertyTypeNameProp = serializedObject.FindProperty("m_PropertyTypeName");
            m_SendVector2ElementsProp = serializedObject.FindProperty("m_SendVector2Elements");
            m_SendVector3ElementsProp = serializedObject.FindProperty("m_SendVector3Elements");

            const string sendElements = "Send Elements";
            s_SendVec2ElementsContent = new GUIContent(sendElements, "Which elements of this Vector2 to send");
            s_SendVec3ElementsContent = new GUIContent(sendElements, "Which elements of this Vector3 to send");
            
            var propTypeName = m_PropertyTypeNameProp.stringValue;
            if (propTypeName != null)
            {
                switch (propTypeName)
                {
                    case "Vector2":
                        m_DrawVector2Filter = true;
                        break;
                    case "Vector3":
                        m_DrawVector3Filter = true;
                        break;
                }
            }

            if (m_Target == null) return;

            m_CachedComponents = m_Target.GetObjectComponents();
            if (m_CachedComponents == null) return;
            
            m_CachedComponentNames = m_CachedComponents.Select(c => c.GetType().Name).ToArray();
            
            var sourceCompRef = m_SourceComponentProp.objectReferenceValue;
            if (sourceCompRef == null) 
                sourceCompRef = m_SourceComponentProp.objectReferenceValue = m_Target.gameObject;
            
            m_ComponentIndex = Array.IndexOf(m_CachedComponentNames, sourceCompRef.GetType().Name);
            if(m_ComponentIndex >= 0)
                GetComponentFieldsAndProperties();

            if (sourceCompRef != null)
            {
                m_ComponentIndex = Array.IndexOf(m_CachedComponentNames, sourceCompRef.GetType().Name);

                var serializedPropName = m_PropertyNameProp.stringValue;
                if(m_PropertyNames != null)
                    m_PropertyIndex = Array.IndexOf(m_PropertyNames, serializedPropName);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("OSC Destination", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_SenderProp);
            EditorGUILayout.PropertyField(m_AddressProp);
            
            EditorGUILayout.LabelField("Property Source", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ObjectProp);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                var objValue = m_ObjectProp.objectReferenceValue;
                if (objValue == null)
                {
                    CleanComponents();
                    return;
                }

                m_PropertyIndex = -1;
                m_ComponentIndex = -1;
                m_CachedComponents = m_Target.GetObjectComponents();
                if (m_CachedComponents != null)
                    m_CachedComponentNames = m_CachedComponents.Select(c => c.GetType().Name).ToArray();
            }

            ComponentDropdown();
            PropertyDropdown();


            serializedObject.ApplyModifiedProperties();
        }


        void ComponentDropdown()
        {
            if (m_CachedComponentNames == null) 
                return;
            
            var newIndex = EditorGUILayout.Popup(k_ComponentContent, m_ComponentIndex, m_CachedComponentNames);
            if (newIndex != m_ComponentIndex)
            {
                m_ComponentIndex = newIndex;
                var compName = m_CachedComponentNames[newIndex];
                if (compName != m_PreviousComponentName)
                    GetComponentFieldsAndProperties();

                m_PropertyIndex = -1;
                m_PreviousComponentName = compName;
                m_PropertyNameProp.stringValue = null;
                m_PropertyTypeNameProp.stringValue = null;
                m_SourceComponentProp.objectReferenceValue = m_CachedComponents[newIndex];
                serializedObject.ApplyModifiedProperties();
                m_Target.SetPropertyFromSerialized();
            }
        }

        void PropertyDropdown()
        {
            if (m_PropertyNames == null) 
                return;
            
            var newIndex = EditorGUILayout.Popup(k_PropertyContent, m_PropertyIndex, m_PropertyNames);
            if (newIndex != m_PropertyIndex)
            {
                m_PropertyIndex = newIndex;
                m_PropertyNameProp.stringValue = m_PropertyNames[m_PropertyIndex];

                var info = m_PropertiesAndFields[m_PropertyIndex];

                Type type;
                var asProp = info as PropertyInfo;
                if (asProp != null)
                {
                    type = asProp.PropertyType;
                    m_Target.Property = asProp;
                }
                else
                {
                    var asField = info as FieldInfo;
                    m_Target.Field = asField;
                    type = asField?.FieldType;
                }

                m_PropertyTypeNameProp.stringValue = type?.Name;

                m_DrawVector3Filter = type == typeof(Vector3);
                m_DrawVector2Filter = type == typeof(Vector2);
            }
            
            if (newIndex >= 0)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(k_PropTypeContent);
                        EditorGUILayout.LabelField(m_PropertyTypeNameProp.stringValue, EditorStyles.whiteLabel);
                    }
                    
                    if (m_DrawVector3Filter)
                        DrawVector3ElementFilter();
                    else if (m_DrawVector2Filter)
                        DrawVector2ElementFilter();
                }
            }
        }

        void DrawVector3ElementFilter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(s_SendVec3ElementsContent);
                EditorGUILayout.PropertyField(m_SendVector3ElementsProp, k_EmptyContent);
            }

            var enumValueIndex = (Vector3ElementFilter) m_SendVector3ElementsProp.enumValueIndex;
            if (enumValueIndex != Vector3ElementFilter.XYZ)
            {
                if (enumValueIndex != m_PreviousVec3FilterEnumValue)
                {
                    var propName = m_PropertyNames[m_PropertyIndex];
                    var filterStr = $"{propName}.{enumValueIndex.ToString().ToLower()}";
                    switch (enumValueIndex)
                    {
                        case Vector3ElementFilter.XY:
                        case Vector3ElementFilter.XZ:
                        case Vector3ElementFilter.YZ:
                            m_FilterHelpLabel = $"sending {filterStr} as a Vector2";
                            break;
                        case Vector3ElementFilter.X:
                        case Vector3ElementFilter.Y:
                        case Vector3ElementFilter.Z:
                            m_FilterHelpLabel = $"sending {filterStr} as a float";
                            break;
                    }
                }

                EditorHelp.DrawBox(m_FilterHelpLabel);
            }
            
            m_PreviousVec3FilterEnumValue = enumValueIndex;
        }
        
        void DrawVector2ElementFilter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(s_SendVec2ElementsContent);
                EditorGUILayout.PropertyField(m_SendVector2ElementsProp, k_EmptyContent);
            }

            var enumValueIndex = (Vector2ElementFilter) m_SendVector2ElementsProp.enumValueIndex;
            if (enumValueIndex != Vector2ElementFilter.XY)
            {
                if (enumValueIndex != m_PreviousVec2FilterEnumValue)
                {
                    var propName = m_PropertyNames[m_PropertyIndex];
                    var filterStr = $"{propName}.{enumValueIndex.ToString().ToLower()}";
                    switch (enumValueIndex)
                    {
                        case Vector2ElementFilter.X:
                        case Vector2ElementFilter.Y:
                            m_FilterHelpLabel = $"sending {filterStr} as a float";
                            break;
                    }
                }

                EditorHelp.DrawBox(m_FilterHelpLabel);
            }
            
            m_PreviousVec2FilterEnumValue = enumValueIndex;
        }

        void GetComponentFieldsAndProperties()
        {
            var comp = m_CachedComponents[m_ComponentIndex];

            var type = comp.GetType();
            var properties = type.GetProperties()
                .Where(p => k_SupportedTypes.Contains(p.PropertyType.FullName)).ToArray();
            
            var fields = type.GetFields()
                .Where(f => k_SupportedTypes.Contains(f.FieldType.FullName)).ToArray();
            
            m_PropertiesAndFields = new MemberInfo[properties.Length + fields.Length];

            int i;
            for (i = 0; i < properties.Length; i++)
            {
                m_PropertiesAndFields[i] = properties[i];
            }

            var fieldsStart = i;
            for (; i < m_PropertiesAndFields.Length; i++)
            {
                m_PropertiesAndFields[i] = fields[i - fieldsStart];
            }
            
            Array.Sort(m_PropertiesAndFields, k_MemberComparer);
            m_PropertyNames = m_PropertiesAndFields.Select(m => m.Name).ToArray();
        }

        void CleanComponents()
        {
            m_CachedComponents = null;
            m_CachedComponentNames = null;
            m_PropertiesAndFields = null;
            m_PropertyNames = null;
            m_ComponentIndex = -1;
            m_PropertyIndex = -1;
            m_PreviousComponentName = null;
            m_PropertyTypeNameProp.stringValue = null;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
