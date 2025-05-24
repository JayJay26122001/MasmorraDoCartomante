
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized] public Card card;
    [System.NonSerialized] public bool EffectAcomplished = false, effectStarted = false;
    [SerializeReference]public List<Condition> Conditions;
    /*public Effect(Card c)
    {
        card = c;
    }*/
    public virtual void Apply()
    {
        effectStarted = true;
    }
    public void resetEffect()
    {
        EffectAcomplished = false;
        effectStarted = false;
    }
    public void EffectEnded()
    {
        EffectAcomplished = true;
        foreach (Effect e in card.Effects)
        {
            if (!e.EffectAcomplished)
            {
                return;
            }
        }
        card.deck.Owner.DiscardCard(card);
    }
}
/*[CustomEditor(typeof(Effect), true)]
public class EffectEditor : Editor
{
    SerializedProperty conditions;

    void OnEnable()
    {
        conditions = serializedObject.FindProperty("Conditions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw everything except Conditions
        DrawPropertiesExcluding(serializedObject, "Conditions");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        for (int i = 0; i < conditions.arraySize; i++)
        {
            var element = conditions.GetArrayElementAtIndex(i);
            string className = element.managedReferenceFullTypename.Split('.').Last();
            EditorGUILayout.PropertyField(element, new GUIContent($"  • {className}"), true);

            if (GUILayout.Button("Remove Condition"))
            {
                conditions.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Condition"))
        {
            ShowConditionTypeSelector();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowConditionTypeSelector()
    {
        var conditionTypes = GetAllDerivedTypes<Condition>();
        GenericMenu menu = new GenericMenu();

        foreach (var type in conditionTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                var newCondition = Activator.CreateInstance(type);
                conditions.arraySize++;
                serializedObject.ApplyModifiedProperties(); // refresh serialized data
                
                var conditionElement = conditions.GetArrayElementAtIndex(conditions.arraySize - 1);
                conditionElement.managedReferenceValue = newCondition;
                serializedObject.ApplyModifiedProperties(); // ensure Unity sees changes
            });
        }

        menu.ShowAsContext();
    }

    private static List<Type> GetAllDerivedTypes<T>()
{
    return AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
        .ToList();
}
}
*/
[Serializable]
public class DealDamage : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DamageMultiplier;
    [SerializeField] bool IgnoreDefense;
    enum Target { Oponent, User }
    [SerializeField]Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Oponent:
                card.deck.Owner.enemy.TakeDamage(GetDamage(), IgnoreDefense);
                break;
            case Target.User:
                card.deck.Owner.TakeDamage(GetDamage(), IgnoreDefense);
                break;
        }
        EffectEnded();
    }
    public int GetDamage()
    {
        return (int)Math.Ceiling(card.deck.Owner.BaseDamage * DamageMultiplier);
    }
}
[Serializable]
public class GainDefense : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DefenseMultiplier;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.AddShield(GetDefense());
        EffectEnded();
    }
    public int GetDefense()
    {
        return (int)Math.Ceiling(card.deck.Owner.BaseDefense * DefenseMultiplier);
    }
}
[Serializable]
public class BuyCards : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public int BuyCardNumber;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.BuyCards(BuyCardNumber);
        Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
    }
}
public class GainEnergy : Effect
{
    public int Amount;
    enum GainTime {WhenPlayed, NextTurn};
    [SerializeField] GainTime time;
    public override void Apply()
    {
        base.Apply();
        switch (time)
        {
            case GainTime.WhenPlayed:
                card.deck.Owner.GainEnergy(Amount);
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
                break;
            case GainTime.NextTurn:
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Start), TurnPhase.PhaseTime.Start, card.deck.Owner.GainEnergy, Amount);
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Start), TurnPhase.PhaseTime.Start, EffectEnded);
                break;
        }
        
    }
}
[Serializable]
public class BuffStatMultiplier : Effect
{
    enum BuffableStats { Attack, Defense }
    [SerializeField] BuffableStats StatToBuff;
    //enum BuffableMethod { Multiply, Add }
    //[SerializeField] BuffableMethod BuffMethod;
    public float MultiplicativeAmount;

    [Header("Duration")]
    public int TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhaseToStop;
    [SerializeField]TurnPhase.PhaseTime StopAtPhase;
    public override void Apply()
    {
        base.Apply();
        GetStatReference() *= MultiplicativeAmount;
        Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner,TurnPhaseToStop), StopAtPhase, () => GetStatReference() /= MultiplicativeAmount);
        Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner,TurnPhaseToStop), StopAtPhase, EffectEnded);
        /*switch (BuffMethod)
        {
            case BuffableMethod.Multiply:
                GetStatReference() *= MultiplicativeAmount;
                Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(TurnPhaseToStop), StopAtPhase, () => GetStatReference() /= MultiplicativeAmount);
                break;
            case BuffableMethod.Add:
                GetStatReference() += MultiplicativeAmount;
                break;
        }*/
    }
    private ref float GetStatReference()
    {
        switch (StatToBuff)
        {
            case BuffableStats.Attack:
                return ref card.deck.Owner.BaseDamageMultiplier;
            case BuffableStats.Defense:
                return ref card.deck.Owner.BaseDefenseMultiplier;
            default:
                throw new System.Exception("Unsupported stat type.");
        }
    }
}