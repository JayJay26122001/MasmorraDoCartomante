using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MigrationScript
{
    /*[MenuItem("Tools/Migrate GainDefense to GainShield")]
    public static void Migrate()
    {
        string[] guids = AssetDatabase.FindAssets("t:Card");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Card card = AssetDatabase.LoadAssetAtPath<Card>(path);

            if (card == null || card.Effects == null)
                continue;

            bool changed = false;

            for (int i = 0; i < card.Effects.Count; i++)
            {
                if (card.Effects[i] is GainDefense oldEffect)
                {
                    var newEffect = new GainShield
                    {
                        DiscardIfAcomplished = oldEffect.DiscardIfAcomplished,
                        Conditions = oldEffect.Conditions,
                        ConfirmationConditions = oldEffect.ConfirmationConditions,
                        //Class effect
                        DefenseMultiplier = oldEffect.DefenseMultiplier
                    };
                    card.Effects[i] = newEffect;
                    changed = true;
                    Debug.Log($"Migrated GainDefense → GainShield in {path}");
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(card);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }*/
}
