using Unity.Cinemachine;
using UnityEngine;

public class CardUIController : MonoBehaviour
{
    public static CardUIController instance;
    public CardDisplay cardPrefab;
    public CinemachineCamera cardsCamera;
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
        HighlightSelectedCard(c);
    }

    public static void OrganizeHandCards(Creature c)
    {
        int totalHandCards = c.hand.Count;
        float fixedSpacing = 2.3f;
        float spacing = (totalHandCards <= 6) ? fixedSpacing : instance.maxTotalWidth / (totalHandCards - 1);
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.02f;
        //float maxHeight = 0.5f;
        float rotationPerCard = 0.1f;
        float yRotation = -rotationPerCard * totalHandCards;

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
            float posZ = i * handVerticalSpacing;
            //float zRotation = startAngle + angleStep * i;
            GameObject cardObject = c.hand[i].cardDisplay.gameObject;
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) /*+ (c.combatSpace.playerHandSpace.up) * posY*/ + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            Vector3 rot = c.combatSpace.playerHandSpace.rotation.eulerAngles; //+ new Vector3(0f, 180f, /*zRotation*/0f);
            LeanTween.move(cardObject, pos, 0.15f).setEaseInOutSine();
            LeanTween.rotate(cardObject, rot, 0.1f).setEaseInOutSine();
            Debug.Log(i);
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
        }
    }

    public static void OrganizePlayedCards(Creature c)
    {
        int totalCardsPlayed = c.playedCards.Count;
        float playedCardsSpacing = 2.5f;
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
        }
    }

    public static void OrganizeStackFlat(SerializableStack<Card> pile, Transform space)
    {
        float spacing = 0.1f;
        int total = pile.Count;

        for (int i = total; i > 0; i--)
        {
            float posY = i * spacing;
            GameObject cardObject = pile.GetVar(i - 1).cardDisplay.gameObject;
            Vector3 pos = (space.up * posY) + space.position;
            Vector3 rot = space.rotation.eulerAngles + new Vector3(-90f, 0f, 180f);
            LeanTween.move(cardObject, pos, 0.15f);
            LeanTween.rotate(cardObject, rot, 0.15f);
            cardObject.transform.SetParent(space);
        }
    }
    public static void OrganizeStack(SerializableStack<Card> pile, Transform space)
    {
        float spacing = 0.1f;
        int total = pile.Count;

        for (int i = total; i > 0; i--)
        {
            float posY = i * spacing;
            GameObject cardObject = pile.GetVar(i - 1).cardDisplay.gameObject;

            Vector3 finalPos = space.position + (space.up * posY);
            Vector3 finalRot = space.rotation.eulerAngles + new Vector3(-90f, 0f, 180f);

            if (cardObject.transform.parent != space)
            {
                Vector3 upPos = cardObject.transform.position + Vector3.up * 20f;
                Vector3 horizontalPos = new Vector3(finalPos.x, upPos.y, finalPos.z);

                LeanTween.move(cardObject, upPos, 0.15f).setEaseInOutSine().setOnComplete(() =>
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


    public static void HighlightSelectedCard(Creature c)
    {
        OrganizeHandCards(c);
        if (c.GetComponent<Player>()?.SelectedCard != null)
        {
            LeanTween.cancel(c.GetComponent<Player>().SelectedCard.cardDisplay.gameObject);
            LeanTween.move(c.GetComponent<Player>().SelectedCard.cardDisplay.gameObject, c.GetComponent<Player>().SelectedCard.cardDisplay.transform.position + 0.2f * c.combatSpace.playerHandSpace.transform.up, 0.15f);
            
        }
    }
}
