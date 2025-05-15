#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Card", menuName = "CardLogic/Card")]
public class Card : ScriptableObject
{
    [SerializeReference]
    public List<Effect> Effects = new List<Effect>();
    public void Setup()
    {
        foreach (Condition.condition c in conditions)
        {
            conds.Add(new Condition(c, this));
        }
    }
    public CardDisplay cardDisplay;
    public Deck deck;
    public string Name;
    public bool hidden, exaust, instantaneous;
    public int cost;
    public enum CardType { Attack, Defense, Mind }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }
    public CardType Type;
    public CardRarity Rarity;
    public string Description;
    public List<Condition.condition> conditions = new List<Condition.condition>();
    List<Condition> conds = new List<Condition>();
    public void CardPlayed() // carta foi jogada na mesa
    {
        if (instantaneous)
        {
            IniciateCardEffect();
            CardUIController.CardsOrganizer(deck.Owner);
            GameplayManager.currentCombat.CombatUI();
        }
    }


    //CONDICIONAIS DA CARTA
    public void CheckConditions() // Checa se as condições para os efeitos da carta foram resolvidas
    {
        foreach (Condition c in conds)
        {
            if (c.ConditionStatus == Condition.ConditionState.Failled)
            {
                ConditionalCardFailled();
                return;
            }
            else if (c.ConditionStatus == Condition.ConditionState.Unsolved)
            {
                return;
            }
        }
        if (hidden)
        {
            hidden = false;
            CardUIController.OrganizeHandCards(deck.Owner);
        }
        CardHadEffect();
    }
    public void ConditionalCardFailled()// caso as condições da carta não tenham sido cumpridas
    {
        foreach (Condition c in conds)
        {
            c.TerminateCondition();
            c.ResetCondition();
        }
        deck.Owner.DiscardCard(this);
    }


    //EFEITOS DA CARTA
    //public UnityEvent CardEffect = new UnityEvent();

    public void IniciateCardEffect() //tenta triggar o efeito da carta se ela não tiver condições, se tiver inicializa as condições
    {
        if (conditions.Count == 0)
        {
            CardHadEffect();
        }
        else
        {
            foreach (Condition c in conds)
            {
                c.InitiateCondition();
            }
        }
    }
    void CardHadEffect() //carta tem efeito
    {
        //CardEffect.Invoke();
        foreach (Effect e in Effects)
        {
            e.Apply();
        }
        foreach (Condition c in conds)
        {
            c.ResetCondition();
        }
        deck.Owner.DiscardCard(this);
    }

    /*public void DamageEnemy(int damage)
    {
        deck.Owner.Enemy.TakeDamage(damage);
    }
    public void AddDefense(int def)
    {
        deck.Owner.AddShield(def);
    }*/
}

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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Draw Effect list
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
        for (int i = 0; i < effects.arraySize; i++)
        {
            var element = effects.GetArrayElementAtIndex(i);
            string className = element.managedReferenceFullTypename.Split(' ').Last().Split('.').Last();
            EditorGUILayout.PropertyField(element, new GUIContent($"=== {className} ==="), true);

            if (GUILayout.Button("Remove Effect"))
            {
                effects.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.Space();
        }

        // Button to add a new effect
        if (GUILayout.Button("Add Effect"))
        {
            ShowTypeSelector();
        }

        

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowTypeSelector()
    {
        var effectTypes = GetAllDerivedTypes<Effect>();
        GenericMenu menu = new GenericMenu();

        foreach (var type in effectTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                var newEffect = Activator.CreateInstance(type);
                if (newEffect is Effect e)
                {
                    e.card = (Card)target;
                }
                effects.arraySize++;
                var effectElement = effects.GetArrayElementAtIndex(effects.arraySize - 1);
                effectElement.managedReferenceValue = newEffect;
                serializedObject.ApplyModifiedProperties();
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
#endif
