using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomPropertyDrawer(typeof(ModularInt))]
public class ModularIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Foldout line
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float lineHeight = EditorGUIUtility.singleLineHeight + 2f; // space between lines
            float yOffset = position.y + lineHeight;

            // Get all the properties
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            SerializedProperty modifiersProp = property.FindPropertyRelative("modifiers");

            // Draw type
            EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), typeProp);
            yOffset += lineHeight;

            // Draw value(s) depending on type
            switch ((ModularVar.ValueType)typeProp.enumValueIndex)
            {
                case ModularVar.ValueType.Fixed:
                    EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),valueProp);
                    yOffset += lineHeight;
                    break;

                case ModularVar.ValueType.Random:
                    EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),minProp);
                    yOffset += lineHeight;

                    EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), maxProp);
                    yOffset += lineHeight;
                    break;

                // Optional: case for future types
                default:
                    EditorGUI.LabelField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),"Unsupported Value Type");
                    yOffset += lineHeight;
                    break;
            }

            // Draw modifiers inside the box
            float modifiersHeight = EditorGUI.GetPropertyHeight(modifiersProp, true);
            EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, modifiersHeight), modifiersProp, true);
            yOffset += modifiersHeight + 2f;

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // Foldout line

        if (property.isExpanded)
        {
            height += EditorGUIUtility.singleLineHeight + 2f; // Type field

            SerializedProperty typeProp = property.FindPropertyRelative("type");

            if (typeProp.enumValueIndex == (int)ModularVar.ValueType.Fixed)
            {
                height += EditorGUIUtility.singleLineHeight + 2f; // Value
            }
            else
            {
                height += (EditorGUIUtility.singleLineHeight + 2f) * 2; // Min + Max
            }

            SerializedProperty modifiersProp = property.FindPropertyRelative("modifiers");
            height += EditorGUI.GetPropertyHeight(modifiersProp, true) + 2f; // Modifiers
        }

        return height;
    }
    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            SerializedProperty modifiersProp = property.FindPropertyRelative("modifiers");

            EditorGUILayout.PropertyField(typeProp);

            switch (typeProp.GetEnumValue<ModularVar.ValueType>())
            {
                case ModularVar.ValueType.Fixed:
                    EditorGUILayout.PropertyField(valueProp);
                    break;

                case ModularVar.ValueType.Random:
                    EditorGUILayout.PropertyField(minProp);
                    EditorGUILayout.PropertyField(maxProp);
                    break;

                default: break;
            }
            EditorGUILayout.PropertyField(modifiersProp, true);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }*/
}
