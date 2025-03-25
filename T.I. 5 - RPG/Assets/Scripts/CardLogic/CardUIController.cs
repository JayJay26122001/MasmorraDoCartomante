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
        int totalHandCards = c.hand.Count;
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
        int totalCardsPlayed = c.playedCards.Count;
        float playedCardsSpacing = 2.5f;
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float positionX = (i - ((totalCardsPlayed - 1) / 2f)) * playedCardsSpacing;
            Transform cardTransform = c.playedCards[i].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.playedCardSpace.right * positionX) + c.combatSpace.playedCardSpace.position;
            cardTransform.rotation = c.combatSpace.playedCardSpace.rotation * Quaternion.Euler(90f, 0f, 0f);
        }
        int totalDiscardCards = c.decks[0].DiscardPile.Count;
        float discardCardsSpacing = 0.1f;
        for (int i = totalDiscardCards; i > 0; i--)
        {
            float positionY = (i * discardCardsSpacing);
            Transform cardTransform = c.decks[0].DiscardPile.ToArray()[i - 1].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.discardPileSpace.up * positionY) + c.combatSpace.discardPileSpace.position;
            cardTransform.rotation = c.combatSpace.discardPileSpace.rotation * Quaternion.Euler(-90f, 0f, 0f);
        }
        int totalBuyingCards = c.decks[0].BuyingPile.Count;
        float buyingCardsSpacing = 0.1f;
        for (int i = totalBuyingCards; i > 0; i--)
        {
            float positionY = (i * buyingCardsSpacing);
            Transform cardTransform = c.decks[0].BuyingPile.ToArray()[i - 1].cardDisplay.transform;
            cardTransform.position = (c.combatSpace.buyingPileSpace.up * positionY) + c.combatSpace.buyingPileSpace.position;
            cardTransform.rotation = c.combatSpace.buyingPileSpace.rotation * Quaternion.Euler(-90f, 0f, 0f);
        }
    }
}
