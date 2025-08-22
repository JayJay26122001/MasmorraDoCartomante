using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ModularFloat))]
[CustomPropertyDrawer(typeof(RecursiveFloat))]
[CustomPropertyDrawer(typeof(ModularInt))]
[CustomPropertyDrawer(typeof(RecursiveInt))]
public class ModularFloatDrawer : PropertyDrawer
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
            SerializedProperty target = property.FindPropertyRelative("target");
            SerializedProperty ObservedPile = property.FindPropertyRelative("ObservedPile");
            SerializedProperty CountOnlyTypes = property.FindPropertyRelative("CountOnlyTypes");
            SerializedProperty MaxReturnedNumber = property.FindPropertyRelative("MaxReturnedNumber");

            // Draw type
            if (typeProp != null)
            {
                EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), typeProp);
                yOffset += lineHeight;
            }

            // Draw value(s) depending on type
            switch ((ModularVar.ValueType)typeProp.enumValueIndex)
            {
                case ModularVar.ValueType.Fixed:
                    if (valueProp != null)
                    {
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), valueProp);
                        yOffset += lineHeight;
                    }
                    break;

                case ModularVar.ValueType.Random:
                    if (minProp != null)
                    {
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), minProp);
                        yOffset += lineHeight;
                    }

                    if (maxProp != null)
                    {
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), maxProp);
                        yOffset += lineHeight;
                    }
                    break;

                case ModularVar.ValueType.CardNumber:
                    if (target != null)
                    {
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), target);
                        yOffset += lineHeight;
                    }
                    if (ObservedPile != null)
                    {
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), ObservedPile);
                        yOffset += lineHeight;
                    }
                    if (MaxReturnedNumber != null)
                    {
                        float numberHeight = EditorGUI.GetPropertyHeight(MaxReturnedNumber, true);
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, numberHeight),MaxReturnedNumber,true);
                        yOffset += numberHeight + 2f;
                    }
                    if (CountOnlyTypes != null)
                    {
                        float TypeHeight = EditorGUI.GetPropertyHeight(CountOnlyTypes, true);
                        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, TypeHeight), CountOnlyTypes, true);
                        yOffset += TypeHeight + 2f;
                    }
                    break;

                // Optional: case for future types
                default:
                    //EditorGUI.LabelField(new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight), "Unsupported Value Type");
                    //yOffset += lineHeight;
                    break;
            }

            // Draw modifiers inside the box
            if (modifiersProp != null)
            {
                float modifiersHeight = EditorGUI.GetPropertyHeight(modifiersProp, true);
                EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, modifiersHeight), modifiersProp, true);
                yOffset += modifiersHeight + 2f;
            }
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

            switch ((ModularVar.ValueType)typeProp.enumValueIndex)
            {
                case ModularVar.ValueType.Fixed:
                    height += EditorGUIUtility.singleLineHeight + 2f; // Value
                    break;

                case ModularVar.ValueType.Random:
                    height += (EditorGUIUtility.singleLineHeight + 2f) * 2; // Min + Max
                    break;

                case ModularVar.ValueType.CardNumber:
                    // target
                    height += EditorGUIUtility.singleLineHeight + 2f;

                    // ObservedPile
                    height += EditorGUIUtility.singleLineHeight + 2f;

                    //Max returned number
                    SerializedProperty maxReturnedNumber = property.FindPropertyRelative("MaxReturnedNumber");
                    if (maxReturnedNumber != null)
                        height += EditorGUI.GetPropertyHeight(maxReturnedNumber, true) + 2f;

                    // CountOnlyTypes (LIST → ask Unity)
                    SerializedProperty countOnlyTypes = property.FindPropertyRelative("CountOnlyTypes");
                    if (countOnlyTypes != null)
                        height += EditorGUI.GetPropertyHeight(countOnlyTypes, true) + 2f;

                    break;
            }

            SerializedProperty modifiersProp = property.FindPropertyRelative("modifiers");
            if (modifiersProp != null)
            {
                height += EditorGUI.GetPropertyHeight(modifiersProp, true) + 2f; // Modifiers
            }


        }

        return height;
    }
}
#endif