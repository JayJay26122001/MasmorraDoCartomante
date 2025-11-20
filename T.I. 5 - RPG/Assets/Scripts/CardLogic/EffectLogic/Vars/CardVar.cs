using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class CardVar : ISerializationCallbackReceiver // usado para obter cartas com certos atributos em lugares que não sejam efeitos de cartas
{
    public enum Target { User, Opponent }
    public enum Pile { Deck, Hand, PlayedPile, DiscardPile, BuyingPile, ExistentDeck, Destroyed }
    [System.Flags]
    public enum Type
    {
        //Nothing = 0,
        Attack = 1 << 0,
        Defense = 1 << 1,
        Mind = 1 << 2,

        //Everything = Attack | Defense | Mind
    }

    [System.Flags]
    public enum Rarity
    {
        //Nothing = 0,
        Common = 1 << 0,
        Uncommon = 1 << 1,
        Rare = 1 << 2,
        Epic = 1 << 3,
        Legendary = 1 << 4,

        //Everything = Common | Uncommon | Rare | Epic | Legendary
    }
    [System.Flags]
    public enum Pack
    {
        //Nothing = 0,
        Normal = 1 << 0,
        Zodiac = 1 << 1,
        EnemyExclusive = 1 << 2,
        MinorArcana = 1 << 3,
        MajorArcana = 1 << 4,
        SandsOfTime = 1 << 5,
        PowerSurge = 1 << 6,

        //Everything = Normal | Zodiac | EnemyExclusive | MinorArcana | MajorArcana
    }
    public Target target;
    public Pile pile;
    public Type type;
    public Rarity rarity;
    public Pack pack;
    public SimpleInt MaxReturnedNumber = new SimpleInt();

    public List<Card> GetCardsWithStats(Creature user)
    {
        Creature t;
        switch (target)
        {
            case Target.User:
                t = user;
                break;
            case Target.Opponent:
                t = user.enemy;
                break;
            default:
                t = null;
                break;

        }
        List<Card> observedPile = GetPile(t);
        List<Card> foundCards = new List<Card>();
        foreach (Card c in observedPile)
        {
            if ((type & (Type)(1 << (int)c.Type)) != 0)
            {
                if ((rarity & (Rarity)(1 << (int)c.Rarity)) != 0)
                {
                    if ((pack & (Pack)(1 << (int)c.Pack)) != 0)
                    {
                        if (foundCards.Count < MaxReturnedNumber.GetValue())
                        {
                            foundCards.Add(c);
                        }
                    }
                }
            }
        }
        return foundCards;
    }
    List<Card> GetPile(Creature t)
    {
        switch (pile)
        {
            case Pile.Hand:
                return t.hand;
            case Pile.PlayedPile:
                return t.playedCards;
            case Pile.DiscardPile:
                return t.decks[0].DiscardPile.ToList();
            case Pile.BuyingPile:
                return t.decks[0].BuyingPile.ToList();
            case Pile.Deck:
                return t.decks[0].cards;
            case Pile.ExistentDeck:
                List<Card> list = new List<Card>();
                foreach (Card c in t.decks[0].cards)
                {
                    if (!t.exausted.Contains(c))
                    {
                        list.Add(c);
                    }
                }
                return list;
            case Pile.Destroyed:
                return t.exausted;
            default: return null;
        }
    }
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
}
[Serializable]
public class ECardVar : CardVar // Deve ser usado apenas em efeitos que envolvam contagem de cartas
{
    public enum CountCard { DontCountThisCard, CountThisCard, CountOnlyThisCard }
    public CountCard CountThisCard;
    public List<Card> GetCardsWithStats(Card ownerCard)
    {
        List<Card> temp = GetCardsWithStats(ownerCard.deck.Owner);
        switch (CountThisCard)
        {
            case CountCard.DontCountThisCard:
                temp.Remove(ownerCard);
                return temp;

            case CountCard.CountThisCard:
                return temp;

            case CountCard.CountOnlyThisCard:
                foreach (Card c in temp)
                {
                    if (c != ownerCard)
                    {
                        temp.Remove(c);
                    }
                }
                return temp;

            default: return null;
        }
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CardVar.Type))][CustomPropertyDrawer(typeof(CardVar.Rarity))][CustomPropertyDrawer(typeof(CardVar.Pack))]
[CustomPropertyDrawer(typeof(ResetProperties.Vars))]
[CustomPropertyDrawer(typeof(NumberOfTriggeredEffects.EffectState))]
public class CardTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.MaskField(
            position,
            label,
            property.intValue,
            property.enumDisplayNames
        );
    }
}
#endif
