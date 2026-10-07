using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Ui.Editor
{
    [CustomPropertyDrawer(typeof(EnumDropdownAttribute))]
    public class EnumDropdownDrawer : PropertyDrawer
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<Type, Dictionary<string, string>> _labels = new Dictionary<Type, Dictionary<string, string>>();

        //============================================================
        // Unity Methods
        //============================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type type = fieldInfo.FieldType;
            if (type.IsArray)
                type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];

            if (property.propertyType != SerializedPropertyType.Enum || type == null || !type.IsEnum || type.IsDefined(typeof(FlagsAttribute), false))
            {
                EditorGUI.LabelField(position, label.text, "일반 enum 필드에만 사용할 수 있습니다.");
                return;
            }

            if (!_labels.TryGetValue(type, out Dictionary<string, string> labels))
            {
                labels = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attribute = field.GetCustomAttribute<EnumDisplayNameAttribute>();
                    labels.Add(field.Name, string.IsNullOrEmpty(attribute?.Name) ? field.Name : attribute.Name);
                }

                _labels.Add(type, labels);
            }

            string[] names = property.enumNames;
            string[] displayNames = new string[names.Length];
            for (int idx = 0; idx < names.Length; idx++)
            {
                displayNames[idx] = labels.TryGetValue(names[idx], out string displayName) ? displayName : names[idx];
            }

            EditorGUI.BeginProperty(position, label, property);
            bool wasMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int selectedIdx = EditorGUI.Popup(position, label.text, property.enumValueIndex, displayNames);
            if (EditorGUI.EndChangeCheck() && selectedIdx >= 0 && selectedIdx < names.Length)
                property.enumValueIndex = selectedIdx;

            EditorGUI.showMixedValue = wasMixed;
            EditorGUI.EndProperty();
        }
    }
}
