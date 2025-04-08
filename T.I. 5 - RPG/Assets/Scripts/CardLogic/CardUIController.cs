using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class CardUIController : MonoBehaviour
{
    public static CardUIController instance;
    public CardDisplay cardPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /*public void InstantiateDeck(Deck deck)
    {

    }*/

    public void InstantiateCard(Card card)
    {
        CardDisplay temp = Instantiate(cardPrefab.gameObject).GetComponent<CardDisplay>();
        temp.SetCard(card);
        if (card.deck != null && card.deck.Owner != null)
        {
            temp.transform.SetParent(card.deck.Owner.transform);
        }
    }

    public static void CardsOrganizer(Creature c) //mudan�a futura
    {
        OrganizeHandCards(c);
        OrganizePlayedCards(c);
        OrganizeVerticalPile(c.decks[0].DiscardPile, c.combatSpace.discardPileSpace);
        OrganizeVerticalPile(c.decks[0].BuyingPile, c.combatSpace.buyingPileSpace);
        HighlightSelectedCard(c);
    }

    public static void OrganizeHandCards(Creature c) 
    {
        int totalHandCards = c.hand.Count;
        float leftLimit = -8.0f;
        float rightLimit = 8.0f;
        float minHandSpacing = 1.0f;
        float maxHandSpacing = 3.0f;
        float handVerticalSpacing = 0.02f;
        int cardsSpacingLimit = 6;
        float multiplierValue = Mathf.Clamp01((totalHandCards - cardsSpacingLimit) / 10f);
        float handSpacing = Mathf.Lerp(maxHandSpacing, minHandSpacing, multiplierValue);
        for (int i = 0; i < totalHandCards; i++)
        {
            float positionX = (i - ((totalHandCards - 1) / 2f)) * handSpacing;
            float positionY = (i * handVerticalSpacing);
            Transform cardTransform = c.hand[i].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.playerHandSpace.right * positionX + c.combatSpace.playerHandSpace.up * positionY) + c.combatSpace.playerHandSpace.position;
            cardTransform.rotation = c.combatSpace.playerHandSpace.rotation * Quaternion.Euler(90f, 0f, 0f);
        }
    }

    public static void OrganizePlayedCards(Creature c)
    {
        int totalCardsPlayed = c.playedCards.Count;
        float playedCardsSpacing = 2.5f;
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float posX = (i - ((totalCardsPlayed - 1) / 2f)) * playedCardsSpacing;
            Transform cardTransform = c.playedCards[i].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.playedCardSpace.right * posX) + c.combatSpace.playedCardSpace.position;
            if (!c.playedCards[i].hidden) //Virar a carta caso ela for Hidden na hora que estiver na mesa de cartas jogadas
            {
                cardTransform.rotation = c.combatSpace.playedCardSpace.rotation * Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                cardTransform.rotation = c.combatSpace.playedCardSpace.rotation * Quaternion.Euler(-90f, 0f, 180f);
            }
        }
    }

    public static void OrganizeVerticalPile(Stack<Card> pile, Transform space)
    {
        Card[] cards = pile.ToArray();
        float spacing = 0.1f;
        int total = pile.Count;

        for (int i = total; i > 0; i--)
        {
            float posY = i * spacing;
            Transform cardTransform = cards[i-1].cardDisplay.transform;
            cardTransform.position = (space.up * posY) + space.position;
            cardTransform.rotation = space.rotation * Quaternion.Euler(-90f, 0f, 180f);
        }
    }

    public static void HighlightSelectedCard(Creature c)
    {
        if (c.GetComponent<Player>()?.SelectedCard != null)
        {
            c.GetComponent<Player>().SelectedCard.cardDisplay.transform.position += 0.2f * c.combatSpace.playerHandSpace.transform.forward;
        }
    }
}
