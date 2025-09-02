using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using System.Linq;
using Unity.Mathematics;
[Serializable]
public abstract class ModularVar : ISerializationCallbackReceiver
{
    [NonSerialized] public Card card;
    public enum ValueType { Fixed, Random, CardNumber, CreatureStat, Infinity}
    public enum Target { User, Opponent }
    //public enum Pile { Hand, PlayedPile, DiscardPile, BuyingPile, Deck }
    public enum StatType {Health, Shield, Energy, Coins}
    public Target target;
    //public Pile ObservedPile;
    //public List<Card.CardType> CountOnlyTypes = new List<Card.CardType>();
    public CardVar ObservedCards;
    public SimpleInt MaxReturnedNumber = new SimpleInt();
    public StatType ObservedStat;
    public void SetCard(Card owner)
    {
        card = owner;
        OnSetCard();
    }

    // Override in children if they need to forward to sub-objects
    protected virtual void OnSetCard() { }
    protected int GetStat()
    {
        int value;
        Creature t = null;
        switch (target)
        {
            case Target.User:
                t = card.deck.Owner;
                break;
            case Target.Opponent:
                t = card.deck.Owner.enemy;
                break;
        }
        switch (ObservedStat)
        {
            case StatType.Health:
                value = t.Health;
                break;
            case StatType.Shield:
                value = t.Shield;
                break;
            case StatType.Energy:
                value = t.Energy;
                break;
            case StatType.Coins:
                value = t.Money;
                break;
            default: return 0;
        }
        return math.clamp(value, 0, MaxReturnedNumber.GetValue());
    }
    protected int GetCardNum()
    {
        return ObservedCards.GetCardsWithStats(card.deck.Owner).Count;
        /*Creature t = null;
        switch (target)
        {
            case Target.User:
                t = card.deck.Owner;
                break;
            case Target.Opponent:
                t = card.deck.Owner.enemy;
                break;
        }
        switch (ObservedPile)
        {
            case Pile.Hand:
                return CheckTypes(t.hand);
            case Pile.PlayedPile:
                return CheckTypes(t.playedCards);
            case Pile.DiscardPile:
                return CheckTypes(t.decks[0].DiscardPile.ToList());
            case Pile.BuyingPile:
                return CheckTypes(t.decks[0].BuyingPile.ToList());
            case Pile.Deck:
                return CheckTypes(t.decks[0].cards);
            default: return 0;
        }*/
    }
    /*int CheckTypes(List<Card> pile)
    {
        if (CountOnlyTypes.Count <= 0) { return math.clamp(pile.Count, 0, MaxReturnedNumber.GetValue()); }
        int num = 0;
        foreach (Card c in pile)
        {
            if (CountOnlyTypes.Contains(c.Type))
            {
                num++;
            }
        }
        return math.clamp(num, 0, MaxReturnedNumber.GetValue());
    }*/
    public void OnBeforeSerialize()
    {

    }
    [SerializeField, HideInInspector] private bool initialized;
    public void OnAfterDeserialize()
    {
        if (!initialized)
        {
            MaxReturnedNumber.type = SimpleVar.ValueType.Infinity;
            initialized = true;
        }
    }
    /*public void OnBeforeSerialize()
    {

    }
    public void OnAfterDeserialize()
    {
        if (!Enum.IsDefined(typeof(SimpleVar.ValueType), MaxReturnedNumber.type))
        {
            MaxReturnedNumber.type = SimpleVar.ValueType.Infinity;
        }
    }*/
}

[Serializable]
public class RecursiveInt : ModularVar
{
    //public RecursiveInt(Card user): base(user) {}

    [SerializeField] ValueType type;
    //[Header("Fixed")]
    public int value;
    //[Header("Random")]
    public int min;
    public int max;

    public virtual int GetValue()
    {
        return GetBaseValue();
    }
    protected int GetBaseValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max + 1);
            case ValueType.CardNumber:
                return GetCardNum();
            case ValueType.Infinity:
                return int.MaxValue;
            case ValueType.CreatureStat:
                return GetStat();
            default: return 0;
        }
    }


}
[Serializable]
public class ModularInt : RecursiveInt
{
    //public ModularInt(Card user) : base(user){}
    public List<ModularModifier> modifiers = new List<ModularModifier>();
    public override int GetValue()
    {
        int value = GetBaseValue();
        foreach (ModularModifier m in modifiers)
        {
            value = m.ApplyOperation(value);
        }
        return value;
    }
    protected override void OnSetCard()
    {
        foreach (var m in modifiers)
            m.SetCard(card);
    }
    // Ensure modifiers know their parent
    /*public void OnAfterDeserialize()
    {
        foreach (var m in modifiers)
            m.Initialize(this);
    }

    public void OnBeforeSerialize() { }*/
}

[Serializable]
public class RecursiveFloat : ModularVar
{
    //public RecursiveFloat(Card user): base(user) {}
    [SerializeField] ValueType type;
    //[Header("Fixed")]
    public float value;
    //[Header("Random")]
    public float min;
    public float max;

    public virtual float GetValue()
    {
        return GetBaseValue();
    }
    protected float GetBaseValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max);
            case ValueType.CardNumber:
                return GetCardNum();
            case ValueType.Infinity:
                return Mathf.Infinity;
            case ValueType.CreatureStat:
                return GetStat();
            default: return 0;
        }
    }
}

[Serializable]
public class ModularFloat : RecursiveFloat
{
    //public ModularFloat(Card user) : base(user){}
    protected override void OnSetCard()
    {
        foreach (var m in modifiers)
            m.SetCard(card);
    }
    public List<ModularModifier> modifiers = new List<ModularModifier>();
    public override float GetValue()
    {
        float value = GetBaseValue();
        foreach (ModularModifier m in modifiers)
        {
            value = m.ApplyOperation(value);
        }
        return value;

    }
    // Ensure modifiers know their parent
    /*public void OnAfterDeserialize()
    {
        foreach (var m in modifiers)
            m.Initialize(this);
    }

    public void OnBeforeSerialize() { }*/
}
[Serializable]
public class ModularModifier
{
    /*public ModularModifier(ModularVar var)
    {
        ownerVar = var;
        IntValue = new RecursiveInt(var.card);
        FloatValue = new RecursiveFloat(var.card);
    }*/
    /*public void Initialize(ModularVar var)
    {
        ownerVar = var;

        if (IntValue == null) IntValue = new RecursiveInt(var.card);
        if (FloatValue == null) FloatValue = new RecursiveFloat(var.card);
    }*/
    //ModularVar ownerVar;
    public enum Equations { DividedBy, MultipliedBy, Add, Subdivide }
    public enum ValueType { Int, Float }
    public Equations operation;
    public ValueType Type;
    public RecursiveInt IntValue;
    public RecursiveFloat FloatValue;
    public void SetCard(Card c)
    {
        if (IntValue != null) IntValue.SetCard(c);
        if (FloatValue != null) FloatValue.SetCard(c);
    }
    public float ApplyOperation(float BaseValue)
    {
        switch (Type)
        {
            case ValueType.Int:
                switch (operation)
                {
                    case Equations.DividedBy:
                        return BaseValue / IntValue.GetValue();
                    case Equations.MultipliedBy:
                        return BaseValue * IntValue.GetValue();
                    case Equations.Add:
                        return BaseValue + IntValue.GetValue();
                    case Equations.Subdivide:
                        return BaseValue - IntValue.GetValue();
                    default: return 0;
                }

            case ValueType.Float:
                switch (operation)
                {
                    case Equations.DividedBy:
                        return BaseValue / FloatValue.GetValue();
                    case Equations.MultipliedBy:
                        return BaseValue * FloatValue.GetValue();
                    case Equations.Add:
                        return BaseValue + FloatValue.GetValue();
                    case Equations.Subdivide:
                        return BaseValue - FloatValue.GetValue();
                    default: return 0;
                }

            default: return 0;
        }

    }
    public int ApplyOperation(int BaseValue)
    {
        switch (Type)
        {
            case ValueType.Int:
                switch (operation)
                {
                    case Equations.DividedBy:
                        return BaseValue / IntValue.GetValue();
                    case Equations.MultipliedBy:
                        return BaseValue * IntValue.GetValue();
                    case Equations.Add:
                        return BaseValue + IntValue.GetValue();
                    case Equations.Subdivide:
                        return BaseValue - IntValue.GetValue();
                    default: return 0;
                }

            case ValueType.Float:
                switch (operation)
                {
                    case Equations.DividedBy:
                        return (int)(BaseValue / FloatValue.GetValue());
                    case Equations.MultipliedBy:
                        return (int)(BaseValue * FloatValue.GetValue());
                    case Equations.Add:
                        return (int)(BaseValue + FloatValue.GetValue());
                    case Equations.Subdivide:
                        return (int)(BaseValue - FloatValue.GetValue());
                    default: return 0;
                }

            default: return 0;
        }

    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ModularModifier))]
public class ModularModifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float svs = EditorGUIUtility.standardVerticalSpacing;

        // Foldout row
        Rect r = new Rect(position.x, position.y, position.width, line);
        property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = position.y + line + svs;

            var operation = property.FindPropertyRelative("operation");
            var typeProp = property.FindPropertyRelative("Type");      // note the capital T
            var intValue = property.FindPropertyRelative("IntValue");
            var floatValue = property.FindPropertyRelative("FloatValue");

            if (operation != null)
            {
                float h = EditorGUI.GetPropertyHeight(operation, false);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), operation, false);
                y += h + svs;
            }

            if (typeProp != null)
            {
                float h = EditorGUI.GetPropertyHeight(typeProp, false);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), typeProp, false);
                y += h + svs;

                // Value by type
                if (typeProp.enumValueIndex == (int)ModularModifier.ValueType.Int && intValue != null)
                {
                    float hVal = EditorGUI.GetPropertyHeight(intValue, false);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, hVal), intValue, false);
                    y += hVal + svs;
                }
                else if (typeProp.enumValueIndex == (int)ModularModifier.ValueType.Float && floatValue != null)
                {
                    float hVal = EditorGUI.GetPropertyHeight(floatValue, false);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, hVal), floatValue, false);
                    y += hVal + svs;
                }
                else
                {
                    float h2 = line;
                    EditorGUI.LabelField(new Rect(position.x, y, position.width, h), "Unsupported Value Type");
                    y += h2 + svs;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float svs = EditorGUIUtility.standardVerticalSpacing;

        float h = line; // foldout

        if (property.isExpanded)
        {
            // Space after foldout
            h += svs;

            var operation = property.FindPropertyRelative("operation");
            var typeProp = property.FindPropertyRelative("Type");
            var intValue = property.FindPropertyRelative("IntValue");
            var floatValue = property.FindPropertyRelative("FloatValue");

            if (operation != null)
                h += EditorGUI.GetPropertyHeight(operation, false) + svs;

            if (typeProp != null)
            {
                h += EditorGUI.GetPropertyHeight(typeProp, false) + svs;

                if (typeProp.enumValueIndex == (int)ModularModifier.ValueType.Int && intValue != null)
                    h += EditorGUI.GetPropertyHeight(intValue, false) + svs;
                else if (typeProp.enumValueIndex == (int)ModularModifier.ValueType.Float && floatValue != null)
                    h += EditorGUI.GetPropertyHeight(floatValue, false) + svs;
                else
                    h += line + svs; // "Unsupported" label
            }
        }

        return h;
    }
}
#endif