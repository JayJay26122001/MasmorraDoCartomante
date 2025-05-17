using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    SerializedProperty effects;

    void OnEnable()
    {
        effects = serializedObject.FindProperty("Effects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Draw the rest of the fields except 'Effects'
        DrawPropertiesExcluding(serializedObject, "Effects");


        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        for (int i = 0; i < effects.arraySize; i++)
        {
            var effectProp = effects.GetArrayElementAtIndex(i);
            if (effectProp.managedReferenceValue == null) continue;

            string className = effectProp.managedReferenceFullTypename.Split('.').Last().Split(' ').Last();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Effect {i+1} - {className}", EditorStyles.boldLabel);

            // Draw all effect fields (excluding Conditions)
            DrawPropertiesExcluding(effectProp, "Conditions");

            // Draw the conditions inside the effect
            var conditionsProp = effectProp.FindPropertyRelative("Conditions");
            EditorGUILayout.LabelField("Conditions", EditorStyles.miniBoldLabel);
            for (int j = 0; j < conditionsProp.arraySize; j++)
            {
                var cond = conditionsProp.GetArrayElementAtIndex(j);
                string conName = cond.managedReferenceFullTypename.Split('.').Last().Split(' ').Last();
                EditorGUILayout.PropertyField(cond, new GUIContent($"Condition {j+1} - {conName}"), true);

                if (GUILayout.Button("Remove Condition"))
                {
                    conditionsProp.DeleteArrayElementAtIndex(j);
                }
            }

            if (GUILayout.Button("Add Condition"))
            {
                ShowConditionSelector(conditionsProp);
            }

            if (GUILayout.Button("Remove Effect"))
            {
                effects.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Effect"))
        {
            ShowEffectSelector();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void ShowEffectSelector()
    {
        var types = GetAllDerivedTypes<Effect>();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                var instance = Activator.CreateInstance(type);
                effects.arraySize++;
                serializedObject.ApplyModifiedProperties();
                var newEffect = effects.GetArrayElementAtIndex(effects.arraySize - 1);
                newEffect.managedReferenceValue = instance;
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    void ShowConditionSelector(SerializedProperty list)
    {
        var types = GetAllDerivedTypes<Condition>();
        var menu = new GenericMenu();

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                var instance = Activator.CreateInstance(type);
                list.arraySize++;
                serializedObject.ApplyModifiedProperties();
                var newCondition = list.GetArrayElementAtIndex(list.arraySize - 1);
                newCondition.managedReferenceValue = instance;
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    static void DrawPropertiesExcluding(SerializedProperty property, params string[] exclude)
    {
        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            if (!exclude.Contains(iterator.name))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
    }

    static System.Collections.Generic.List<Type> GetAllDerivedTypes<T>()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();
    }
}