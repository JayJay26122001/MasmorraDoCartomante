using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CardUIController : MonoBehaviour
{
    public static CardUIController instance;
    public CardDisplay cardPrefab;
    public CinemachineCamera cardsCamera;
    public float maxTotalWidth = 12.0f;
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
        OrganizeStackFlat(c.decks[0].DiscardPile, c.combatSpace.discardPileSpace);
        OrganizeStack(c.decks[0].BuyingPile, c.combatSpace.buyingPileSpace);
        //HighlightSelectedCard(c);
    }

    public static void OrganizeHandCards(Creature c)
    {
        int totalHandCards = c.hand.Count;
        float fixedSpacing = 2f;
        float spacing = instance.maxTotalWidth / (totalHandCards - 1);
        if(spacing > fixedSpacing)
        {
            spacing = fixedSpacing;
        }
        //spacing = (spacing > 2f) ? fixedSpacing : instance.maxTotalWidth / (totalHandCards - 1);
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.02f;
        //float maxHeight = 0.5f;
        float rotationPerCard = 0.1f;
        float yRotation = -rotationPerCard * totalHandCards;
        int totalIterations = 1;
        float curvatureHeight = 0.15f;

        foreach (var card in c.hand)
        {
            card.cardDisplay.isReadyToMove = false;
        }

        /*float arcAngle = 25f;
        float angleStep = (totalHandCards > 1) ? arcAngle / (totalHandCards - 1) : 0f;
        float startAngle = -arcAngle / 2f;*/

        //c.combatSpace.playerHandSpace.localRotation = Quaternion.Euler(c.combatSpace.playerHandSpace.localRotation.eulerAngles.x, c.combatSpace.playerHandSpace.localRotation.eulerAngles.y, zRotation);
        if (c.GetComponent<Player>() != null)
        {
            //c.combatSpace.playerHandSpace.transform.LookAt(Camera.main.transform.position);
            c.combatSpace.playerHandSpace.transform.rotation = instance.cardsCamera.transform.rotation * Quaternion.Euler(0f, yRotation, 0f);
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            /*float distanceFromCenter = Mathf.Abs(i - index);
            float maxDistance = Mathf.Floor(totalHandCards / 2f);
            float posY = maxHeight * (1 - (distanceFromCenter / maxDistance));*/
            float normalized = (index != 0f) ? (i - index) / index : 0f;
            float posY = (-Mathf.Pow(normalized, 2) + 1) * curvatureHeight;
            float posZ = i * handVerticalSpacing;
            //float zRotation = startAngle + angleStep * i;
            Card currentCard = c.hand[i];
            GameObject cardObject = currentCard.cardDisplay.gameObject;
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) + (c.combatSpace.playerHandSpace.up) * posY + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            Vector3 rot = c.combatSpace.playerHandSpace.rotation.eulerAngles; //+ new Vector3(0f, 180f, /*zRotation*/0f);
            LeanTween.rotate(cardObject, rot, 0.1f).setEaseInOutSine();
            var moveTween = LeanTween.move(cardObject, pos, 0.15f).setEaseInOutSine();
            moveTween.setOnComplete(() =>
            {
                currentCard.cardDisplay.UpdatePosition();
                totalIterations++;
                if (totalIterations >= totalHandCards)
                {
                    foreach (var card in c.hand)
                    {
                        card.cardDisplay.isReadyToMove = true;
                    }
                }
            });
        }
    }

    /*public static void OrganizeHandCardsWhenHighlighted(Creature c, Card highlightedCard)
    {
        int totalHandCards = c.hand.Count;
        float fixedSpacing = 2f;
        float spacing = instance.maxTotalWidth / (totalHandCards - 1);
        if (spacing > fixedSpacing)
        {
            spacing = fixedSpacing;
        }
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.02f;
        float rotationPerCard = 0.1f;
        float yRotation = -rotationPerCard * totalHandCards;
        int totalIterations = 1;
        int highlightedIndex = c.hand.IndexOf(highlightedCard);
        float separation = 2f;
        if (totalHandCards <= 5)
        {
            separation = 1f;
        }
        else
        {
            float minSeparation = 1f;
            float maxSeparation = 2.5f;
            int minCards = 6;
            int maxCards = 15;
            float t = Mathf.InverseLerp(minCards, maxCards, totalHandCards);
            separation = Mathf.Lerp(minSeparation, maxSeparation, t);
        }
        foreach (var card in c.hand)
        {
            card.cardDisplay.isReadyToMove = false;
        }   
        if (c.GetComponent<Player>() != null)
        {
            c.combatSpace.playerHandSpace.transform.rotation = instance.cardsCamera.transform.rotation * Quaternion.Euler(0f, yRotation, 0f);
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            if (i < highlightedIndex)
            {
                posX -= separation;
            }
            else if (i > highlightedIndex) 
            { 
                posX += separation;
            }
            float posZ = i * handVerticalSpacing;
            Card currentCard = c.hand[i];
            GameObject cardObject = currentCard.cardDisplay.gameObject;
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            Vector3 rot = c.combatSpace.playerHandSpace.rotation.eulerAngles;
            LeanTween.rotate(cardObject, rot, 0.1f).setEaseInOutSine();
            var moveTween = LeanTween.move(cardObject, pos, 0.15f).setEaseInOutSine();
            moveTween.setOnComplete(() =>
            {
                currentCard.cardDisplay.UpdatePosition();
                totalIterations++;
                if (totalIterations >= totalHandCards)
                {
                    foreach (var card in c.hand)
                    {
                        card.cardDisplay.isReadyToMove = true;
                    }
                }
            });
        }
    }*/

    public static void OrganizePlayedCards(Creature c)
    {
        int totalCardsPlayed = c.playedCards.Count;
        float playedCardsSpacing = 2.5f;
        foreach (var card in c.playedCards)
        {
            card.cardDisplay.isReadyToMove = false;
        }
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float posX = (i - ((totalCardsPlayed - 1) / 2f)) * playedCardsSpacing;
            GameObject cardObject = c.playedCards[i].cardDisplay.gameObject;
            Vector3 pos = (c.combatSpace.playedCardSpace.right * posX) + c.combatSpace.playedCardSpace.position;
            Vector3 rot;
            if (!c.playedCards[i].hidden) //Virar a carta caso ela for Hidden na hora que estiver na mesa de cartas jogadas
            {
                //rot = c.combatSpace.playedCardSpace.rotation * Quaternion.Euler(90f, 0f, 0f);
                rot = c.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(90f, 0f, 0f);
            }
            else
            {
                //rot = c.combatSpace.playedCardSpace.rotation * Quaternion.Euler(-90f, 0f, 180f);
                rot = c.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(-90f, 180f, 0);
            }
            LeanTween.move(cardObject, pos, 0.15f).setEaseInOutSine();
            LeanTween.rotate(cardObject, rot, 0.15f).setEaseInOutSine();
            cardObject.transform.SetParent(c.combatSpace.playedCardSpace);
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
        }
    }

    public static void OrganizeStackFlat(SerializableStack<Card> pile, Transform space)
    {
        float spacing = 0.1f;

        for (int i = 0; i < pile.Count; i++)
        {
            float posY = i * spacing;
            GameObject cardObject = pile.GetVar(pile.Count - i - 1).cardDisplay.gameObject;
            Vector3 pos = (space.up * posY) + space.position;
            Vector3 rot = space.rotation.eulerAngles + new Vector3(-90f, 0f, 180f);
            LeanTween.move(cardObject, pos, 0.15f);
            LeanTween.rotate(cardObject, rot, 0.15f);
            cardObject.transform.SetParent(space);
        }
    }
    public static void OrganizeStack(SerializableStack<Card> pile, Transform space)
    {
        float spacing = 0.03f;
        for (int i = 0; i < pile.Count; i++)
        {
            float posY = i * spacing;
            GameObject cardObject = pile.GetVar(pile.Count - i - 1).cardDisplay.gameObject;

            Vector3 finalPos = space.position + (space.up * posY);
            Vector3 finalRot = space.rotation.eulerAngles + new Vector3(-90f, 0f, 180f);

            if (cardObject.transform.parent != space)
            {
                Vector3 upPos = cardObject.transform.position + Vector3.up * 20f;
                Vector3 horizontalPos = new Vector3(finalPos.x, upPos.y, finalPos.z);

                LeanTween.move(cardObject, upPos, 0.15f + i*0.02f).setEaseInOutSine().setOnComplete(() =>
                {
                    cardObject.transform.position = upPos;
                    LeanTween.rotate(cardObject, finalRot, 0.15f).setEaseInOutSine();
                    LeanTween.move(cardObject, horizontalPos, 0.15f).setEaseInOutSine().setOnComplete(() =>
                    {
                        cardObject.transform.position = horizontalPos;

                        LeanTween.move(cardObject, finalPos, 0.15f).setEaseInOutSine();
                        cardObject.transform.SetParent(space);
                    });
                });
            }
        }
    }


    /*public static void HighlightSelectedCard(Creature c)
    {
        OrganizeHandCards(c);
        if (c.GetComponent<Player>()?.SelectedCard != null)
        {
            LeanTween.cancel(c.GetComponent<Player>().SelectedCard.cardDisplay.gameObject);
            LeanTween.move(c.GetComponent<Player>().SelectedCard.cardDisplay.gameObject, c.GetComponent<Player>().SelectedCard.cardDisplay.transform.position + 0.1f * c.combatSpace.playerHandSpace.transform.up, 0.15f);
            
        }
    }*/
}
