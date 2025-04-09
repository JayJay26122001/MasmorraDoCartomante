using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CardUIController : MonoBehaviour
{
    public static CardUIController instance;
    public CardDisplay cardPrefab;
    public float maxTotalWidth = 15.0f;
    private void Awake()
    {
        instance = this;
        /*if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);*/
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
        float fixedSpacing = 2.0f;
        float spacing = (totalHandCards  <= 6) ? fixedSpacing : instance.maxTotalWidth / (totalHandCards - 1);
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.02f;
        float rotationPerCard = 0.1f;
        float yRotation = -rotationPerCard * totalHandCards;
        //c.combatSpace.playerHandSpace.localRotation = Quaternion.Euler(c.combatSpace.playerHandSpace.localRotation.eulerAngles.x, c.combatSpace.playerHandSpace.localRotation.eulerAngles.y, zRotation);
        if(c.GetComponent<Player>() != null)
        {
            //c.combatSpace.playerHandSpace.transform.LookAt(Camera.main.transform.position);
            c.combatSpace.playerHandSpace.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0f, 180f + yRotation, 0f);
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            float posY = (i * handVerticalSpacing);
            Transform cardTransform = c.hand[i].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.playerHandSpace.right * posX + (-c.combatSpace.playerHandSpace.forward) * posY) + c.combatSpace.playerHandSpace.position;
            cardTransform.rotation = c.combatSpace.playerHandSpace.rotation * Quaternion.Euler(0f, 180f, 0f);
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
        OrganizeHandCards(c);
        if (c.GetComponent<Player>()?.SelectedCard != null)
        {
            c.GetComponent<Player>().SelectedCard.cardDisplay.transform.position += 0.2f * c.combatSpace.playerHandSpace.transform.up;
        }
    }
}
