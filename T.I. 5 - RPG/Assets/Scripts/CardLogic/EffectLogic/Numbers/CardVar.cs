using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardVar
{
    public enum Target { User, Opponent }
    public enum Pile { Deck, Hand, PlayedPile, DiscardPile, BuyingPile }
    public enum Type { Any, Attack = Card.CardType.Attack, Defense = Card.CardType.Defense, Mind = Card.CardType.Mind}
    public enum Rarity 
    { 
        Any, 
        Common = Card.CardRarity.Common ,
        Uncommon = Card.CardRarity.Uncommon, 
        Rare = Card.CardRarity.Rare, 
        Epic = Card.CardRarity.Epic, 
        Legendary = Card.CardRarity.Legendary 
    }
    public enum Pack
    {
        Any,
        Normal = Card.CardPack.Normal,
        Zodiac = Card.CardPack.Zodiac,
        EnemyExclusive = Card.CardPack.EnemyExclusive,
        MinorArcana = Card.CardPack.MinorArcana,
        MajorArcana = Card.CardPack.MajorArcana
    }
    public Target target;
    public Pile pile;
    public Type type;
    public Rarity rarity;
    public Pack pack;

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
                t= null;
                break;

        }
        List<Card> observedPile = GetPile(t);
        List<Card> foundCards = new List<Card>();
        foreach (Card c in t.decks[0].cards)
        {
            if (type == Type.Any || (int)c.Type == (int)type-1)
            {
                if (rarity == Rarity.Any || (int)c.Rarity == (int)rarity - 1)
                {
                    if (pack == Pack.Any || (int)c.Pack == (int)pack - 1)
                    {
                        foundCards.Add(c);
                    }
                }
            }
        }
        return foundCards;
    }
    List<Card> GetPile( Creature t)
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
            default: return null;
        }
    }
}
