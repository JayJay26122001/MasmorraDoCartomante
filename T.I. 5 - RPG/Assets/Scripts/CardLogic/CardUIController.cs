using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEngine.Splines.SplineInstantiate;
using TMPro;
//using static UnityEditor.PlayerSettings;

public class CardUIController : MonoBehaviour
{
    public static CardUIController instance;
    public CardDisplay cardPrefab;
    public CinemachineCamera cardsCamera;
    public float maxTotalWidth = 12.0f;
    public float instantTimeAnim, smallTimeAnim, mediumTimeAnim, bigTimeAnim, highlightTimeAnim, delayTimeAnim, rotHandTimeAnim;
    public GameObject enemyCardAppearVfx, puffVfx;
    public TextMeshPro buyingPileText, discardPileText;
    public Transform buyingPilePos, discardPilePos, cardLookPos;
    public Card highlightedCard { get; protected set; }

    public void SetHighlightedCard(Card card)
    {
        if (highlightedCard != card)
        {
            if (highlightedCard != null)
            {
                highlightedCard.cardDisplay.UnhighlightCard();
            }
            highlightedCard = card;
            if (highlightedCard != null)
            {
                highlightedCard.cardDisplay.HighlightCard();
            }
        }
    }
    private void Awake()
    {
        instance = this;
    }

    public CardDisplay InstantiateCard(Card card)
    {
        CardDisplay temp = Instantiate(cardPrefab.gameObject, new Vector3(0, 25, 0), Quaternion.identity).GetComponent<CardDisplay>();
        temp.SetCard(card);
        if (card.deck != null && card.deck.Owner != null)
        {
            temp.transform.SetParent(card.deck.Owner.transform);
            foreach (Effect e in temp.cardData.Effects)
            {
                e.SetCard(temp.cardData);
                //e.card = temp.cardData;
                foreach (Condition c in e.Conditions)
                {
                    c.effect = e;
                }
                foreach (ConfirmationCondition c in e.ConfirmationConditions)
                {
                    c.effect = e;
                }
            }
        }
        return temp;
    }

    public static void AttCardDescription(Creature creature)
    {
        foreach (Card c in creature.hand)
        {
            if (c.cardDisplay != null)
            {
                c.cardDisplay.UpdateCard();
            }
        }
        foreach (Card c in creature.playedCards)
        {
            if (c.cardDisplay != null)
            {
                c.cardDisplay.UpdateCard();
            }
        }
        foreach (Deck deck in creature.decks)
        {
            foreach (Card c in deck.BuyingPile.ToList())
            {
                if (c.cardDisplay != null)
                {
                    c.cardDisplay.UpdateCard();
                }
            }
            foreach (Card c in deck.DiscardPile.ToList())
            {
                if (c.cardDisplay != null)
                {
                    c.cardDisplay.UpdateCard();
                }
            }
        }
        /*foreach (Deck deck in creature.decks)
        {
            foreach (Card c in deck.allCards)
            {
                if (c.cardDisplay != null)
                {
                    c.cardDisplay.UpdateCard();
                }
            }
        }*/
    }

    public static void AttDeckCard(Creature creature)
    {
        foreach (Deck deck in creature.decks)
        {
            foreach (Card c in deck.cards)
            {
                if (c.cardDisplay != null)
                {
                    c.cardDisplay.UpdateCard();
                }
            }
        }
        if (creature is Boss b)
        {
            foreach (Card c in b.BonusDeck.cards)
            {
                if (c.cardDisplay != null)
                {
                    c.cardDisplay.UpdateCard();
                }
            }
        }
    }

    public static void CardsOrganizer(Creature c) //mudan�a futura
    {
        OrganizeHandCards(c);
        OrganizePlayedCards(c);
        OrganizeStackFlat(c.decks[0].DiscardPile, c.combatSpace.discardPileSpace);
        OrganizeStack(c.decks[0].BuyingPile, c.combatSpace.buyingPileSpace);
        if (c is Boss b)
        {
            OrganizeStackFlat(b.BonusDeck.DiscardPile, c.combatSpace.discardPileSpace);
            OrganizeStack(b.BonusDeck.BuyingPile, c.combatSpace.buyingPileSpace);
        }
    }

    public static void OrganizeHandCards(Creature c)
    {
        int totalHandCards = c.hand.Count;
        if (totalHandCards == 0) return;
        float fixedSpacing = 2f;
        float spacing = instance.maxTotalWidth / (totalHandCards - 1);
        if (spacing > fixedSpacing)
        {
            spacing = fixedSpacing;
        }
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.003f;
        float curvatureHeight = 0.15f;

        if (c.GetComponent<Player>() != null)
        {
            c.combatSpace.playerHandSpace.transform.rotation = instance.cardsCamera.transform.rotation;
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            float normalized = (index != 0f) ? (i - index) / index : 0f;
            float posY = (-Mathf.Pow(normalized, 2) + 1) * curvatureHeight;
            float posZ = i * handVerticalSpacing;
            Card currentCard = c.hand[i];
            GameObject cardObject = currentCard.cardDisplay.gameObject;
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) + (c.combatSpace.playerHandSpace.up) * posY + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            Vector3 rot = c.combatSpace.playerHandSpace.rotation.eulerAngles;
            LeanTween.rotate(cardObject, rot, instance.rotHandTimeAnim).setEaseInOutSine();
            var moveTween = LeanTween.move(cardObject, pos, instance.smallTimeAnim).setEaseInOutSine();
            moveTween.setOnComplete(() =>
            {
                currentCard.cardDisplay.UpdatePosition();
            });
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }
        AttCardDescription(c);
    }

    public static void OrganizeHandCardsWhenHighlighted(Creature c)
    {
        int totalHandCards = c.hand.Count;
        if (totalHandCards == 0) return;
        float fixedSpacing = 2f;
        float spacing = instance.maxTotalWidth / (totalHandCards - 1);
        if (spacing > fixedSpacing)
        {
            spacing = fixedSpacing;
        }
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.003f;
        float curvatureHeight = 0.15f;
        int highlightIndex = c.hand.IndexOf(instance.highlightedCard);
        if (highlightIndex < 0) highlightIndex = totalHandCards - 1;
        int[] offsets = GetOffsetMultipliers(totalHandCards, highlightIndex);
        if (c.GetComponent<Player>() != null)
        {
            c.combatSpace.playerHandSpace.transform.rotation = instance.cardsCamera.transform.rotation;
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            float normalized = (index != 0f) ? (i - index) / index : 0f;
            float posY = (-Mathf.Pow(normalized, 2) + 1) * curvatureHeight;
            float posZ = handVerticalSpacing * offsets[i];
            Card currentCard = c.hand[i];
            GameObject cardObject = currentCard.cardDisplay.gameObject;
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) + (c.combatSpace.playerHandSpace.up) * posY + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            LeanTween.move(cardObject, pos, instance.highlightTimeAnim).setEaseInOutSine();
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }

    }
    public static int[] GetOffsetMultipliers(int cardCount, int selectedIndex)
    {
        int[] result = new int[cardCount];
        int maxValue = cardCount - 1;
        List<int> lower = new List<int>(), higher = new List<int>();

        for (int i = 0; i < cardCount; i++)
        {
            if (i < selectedIndex)
            {
                result[i] = i;
                lower.Add(result[i]);
            }
            else if (i > selectedIndex)
            {
                result[i] = Mathf.Abs(i - maxValue);
                higher.Add(result[i]);
            }
        }
        int greatest = lower.Count;
        if (higher.Count > greatest) greatest = higher.Count;

        result[selectedIndex] = greatest;

        return result;
    }
    /*public static int[] GetOffsetMultipliers(int cardCount, int selectedIndex)
    {
        int[] result = new int[cardCount];
        int maxValue = cardCount - 1;
    
        for (int i = 0; i < cardCount; i++)
        {
            result[i] = Mathf.Max(0, maxValue - Mathf.Abs(i - selectedIndex));
        }
    
        return result;
    }*/

    /*public static void OrganizeHandCardsWhenUnhighlighted(Creature c)
    {
        int totalHandCards = c.hand.Count;
        float fixedSpacing = 2f;
        float spacing = instance.maxTotalWidth / (totalHandCards - 1);
        if (spacing > fixedSpacing)
        {
            spacing = fixedSpacing;
        }
        float index = (totalHandCards - 1) / 2f;
        float handVerticalSpacing = 0.003f;
        float curvatureHeight = 0.15f;
        int highlightIndex = c.hand.IndexOf(instance.highlightedCard);
        if (c.GetComponent<Player>() != null)
        {
            c.combatSpace.playerHandSpace.transform.rotation = instance.cardsCamera.transform.rotation;
        }
        for (int i = 0; i < totalHandCards; i++)
        {
            float posX = (i - index) * spacing;
            float normalized = (index != 0f) ? (i - index) / index : 0f;
            float posY = (-Mathf.Pow(normalized, 2) + 1) * curvatureHeight;
            float posZ = handVerticalSpacing * -(Mathf.Abs(highlightIndex - i) - highlightIndex);
            Card currentCard = c.hand[i];
            GameObject cardObject = currentCard.cardDisplay.gameObject;
            cardObject.transform.SetParent(c.combatSpace.playerHandSpace);
            Vector3 pos = (c.combatSpace.playerHandSpace.right * posX) + (c.combatSpace.playerHandSpace.up) * posY + (-c.combatSpace.playerHandSpace.forward) * posZ + c.combatSpace.playerHandSpace.position;
            LeanTween.move(cardObject, pos, 0.05f).setEaseInOutSine();
        }
    }*/

    public static void OrganizePlayedCards(Creature c)
    {
        int totalCardsPlayed = c.playedCards.Count;
        float playedCardsSpacing = 3f;
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float posX = (i - ((totalCardsPlayed - 1) / 2f)) * playedCardsSpacing;
            GameObject cardObject = c.playedCards[i].cardDisplay.gameObject;
            Vector3 pos = (c.combatSpace.playedCardSpace.right * posX) + c.combatSpace.playedCardSpace.position;
            Vector3 rot = c.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(90f, 0f, 0f);
            if (c.GetComponent<Player>() != null)
            {
                LeanTween.rotate(cardObject, rot, instance.smallTimeAnim).setEaseInOutSine();
                if (i == totalCardsPlayed - 1)
                {
                    LeanTween.move(cardObject, pos, instance.smallTimeAnim).setEaseInOutSine().setOnComplete(() =>
                    {
                        //PlayCardVFX(instance.puffVfx, pos, rot, 0.5f);
                    });
                }
                else
                {
                    LeanTween.move(cardObject, pos, instance.smallTimeAnim).setEaseInOutSine();
                }
                /*LeanTween.move(cardObject, pos, 0.15f).setEaseInOutSine().setOnComplete(() =>
                {
                    PlayCardVFX(instance.puffVfx, pos, rot, 0.5f);
                });*/
                cardObject.transform.SetParent(c.combatSpace.playedCardSpace);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            }
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }
        AttCardDescription(c);
    }

    public static void OrganizeBoughtPackCards(CardPack pack)
    {
        int totalCardsPlayed = pack.cardsInstances.Count;
        float playedCardsSpacing = 3f;
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float posX = (i - ((totalCardsPlayed - 1) / 2f)) * playedCardsSpacing;
            GameObject cardObject = pack.cardsInstances[i].cardDisplay.gameObject;
            Vector3 pos = (GameplayManager.instance.player.combatSpace.playedCardSpace.right * posX) + GameplayManager.instance.player.combatSpace.playedCardSpace.position;
            Vector3 rot = GameplayManager.instance.player.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(90f, 0f, 0f);
            if (GameplayManager.instance.player.GetComponent<Player>() != null)
            {
                LeanTween.move(cardObject, pos, instance.smallTimeAnim).setEaseInOutSine();
                LeanTween.rotate(cardObject, rot, instance.smallTimeAnim).setEaseInOutSine();
                cardObject.transform.SetParent(GameplayManager.instance.player.combatSpace.playedCardSpace);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            }
        }
    }

    public static void OrganizeAllDeckCards(List<CardDisplay> cards)
    {
        int totalCards = cards.Count();
        int maxCardsPerRow = 7;
        float spacingX = 2.75f;
        float spacingZ = 4f;
        for (int i = 0; i < totalCards; i++)
        {
            int row = i / maxCardsPerRow;
            int column = i % maxCardsPerRow;
            float posX = (column - (Mathf.Min(maxCardsPerRow, totalCards - row * maxCardsPerRow) - 1) / 2f) * spacingX;
            float posZ = -row * spacingZ;
            GameObject cardObject = cards[i].gameObject;
            Vector3 pos = (GameplayManager.instance.player.combatSpace.playedCardSpace.right * posX) + GameplayManager.instance.player.combatSpace.playedCardSpace.forward * posZ + GameplayManager.instance.player.combatSpace.playedCardSpace.position;
            Vector3 rot = GameplayManager.instance.player.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(90f, 0f, 0f);
            if (GameplayManager.instance.player.GetComponent<Player>() != null)
            {
                LeanTween.cancel(cardObject);
                LeanTween.move(cardObject, pos, instance.smallTimeAnim).setEaseInOutSine();
                LeanTween.rotate(cardObject, rot, instance.smallTimeAnim).setEaseInOutSine();
                cardObject.transform.SetParent(GameplayManager.instance.player.combatSpace.playedCardSpace);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            }
        }
    }
    public static void OrganizeEnemyPlayedCards(Creature c)
    {
        int totalCardsPlayed = c.playedCards.Count;
        float playedEnemyCardsSpacing = 3f;
        for (int i = 0; i < totalCardsPlayed; i++)
        {
            float posX = (i - ((totalCardsPlayed - 1) / 2f)) * playedEnemyCardsSpacing;
            Vector3 finalPos = (c.combatSpace.playedCardSpace.right * posX) + c.combatSpace.playedCardSpace.position;
            Vector3 spawnPos = finalPos + Vector3.up * 5f;
            Vector3 upPos = spawnPos + Vector3.forward * -25f;
            Vector3 lookRot = c.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(20f, 0f, 0f);
            Vector3 rot = c.combatSpace.playedCardSpace.rotation.eulerAngles + new Vector3(90f, 0f, 0f);
            GameObject cardObject = c.playedCards[i].cardDisplay.gameObject;
            if (i == totalCardsPlayed - 1)
            {
                cardObject.transform.position = spawnPos;
                cardObject.GetComponent<CardDisplay>().CardDisapearanceAnimation(false);
                LeanTween.rotate(cardObject, rot, instance.instantTimeAnim).setEaseInOutSine();
                LeanTween.delayedCall(cardObject, instance.mediumTimeAnim, () =>
                {
                    LeanTween.rotate(cardObject, lookRot, instance.bigTimeAnim).setEaseInOutSine();
                    LeanTween.move(cardObject, instance.cardLookPos, instance.bigTimeAnim).setEaseOutCubic().setOnComplete(() =>
                    {
                        LeanTween.delayedCall(cardObject, instance.bigTimeAnim, () =>
                        {
                            LeanTween.rotate(cardObject, rot, instance.mediumTimeAnim).setEaseInOutSine();
                            LeanTween.move(cardObject, finalPos, instance.mediumTimeAnim).setEaseInCubic().setOnComplete(() =>
                            {
                                //PlayCardVFX(instance.puffVfx, finalPos, rot, 0.5f);
                            });
                        });
                    });
                });
                cardObject.transform.SetParent(c.combatSpace.playedCardSpace);
            }
            else
            {
                LeanTween.move(cardObject, finalPos, instance.mediumTimeAnim).setEaseInOutSine();
            }
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }
    }

    public static void PlayCardVFX(GameObject cardVfx, Vector3 pos)
    {
        if (cardVfx != null)
        {
            Quaternion rotation = Quaternion.Euler(cardVfx.transform.rotation.eulerAngles);
            GameObject vfx = Instantiate(cardVfx, pos + Vector3.forward * 1f + Vector3.up * 0.5f, rotation);
            Destroy(vfx, 0.5f);
        }
    }


    public static void OrganizeStackFlat(SerializableStack<Card> pile, Transform space)
    {
        float spacing = 0.1f;
        for (int i = 0; i < pile.Count; i++)
        {
            float posY = i * spacing;
            Card card = pile.GetVar(pile.Count - i - 1);
            GameObject cardObject = card.cardDisplay.gameObject;
            Vector3 pos = (space.up * posY) + space.position;
            Vector3 rot = space.rotation.eulerAngles + new Vector3(-90f, 0f, 180f);
            //instance.ChangeDiscardPileTextValue();
            if (card.deck.Owner != GameplayManager.instance.player)
            {
                cardObject.GetComponent<CardDisplay>().CardDisapearanceAnimation(true);
                LeanTween.move(cardObject, pos, instance.smallTimeAnim).setDelay(instance.delayTimeAnim + 0.25f);
                LeanTween.rotate(cardObject, rot, instance.smallTimeAnim).setDelay(instance.delayTimeAnim + 0.25f);
            }
            else
            {
                LeanTween.move(cardObject, pos, instance.smallTimeAnim);
                LeanTween.rotate(cardObject, rot, instance.smallTimeAnim);
            }
            cardObject.transform.SetParent(space);
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }
        instance.ChangeDiscardPileTextValue();
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
                /*if(pile == GameplayManager.currentCombat.combatents[0].decks[0].BuyingPile)
                {
                    ChangePileTextValues(buyingPilePos);
                }
                else if(pile == GameplayManager.currentCombat.combatents[0].decks[0].DiscardPile)
                {
                    ChangePileTextValues(discardPilePos);
                }*/
                instance.ChangeBuyingPileTextValue();
                LeanTween.move(cardObject, upPos, instance.smallTimeAnim + i * 0.02f).setEaseInOutSine().setOnComplete(() =>
                {
                    cardObject.transform.position = upPos;
                    LeanTween.rotate(cardObject, finalRot, instance.smallTimeAnim).setEaseInOutSine();
                    LeanTween.move(cardObject, horizontalPos, instance.smallTimeAnim).setEaseInOutSine().setOnComplete(() =>
                    {
                        cardObject.transform.position = horizontalPos;

                        LeanTween.move(cardObject, finalPos, instance.smallTimeAnim).setEaseInOutSine();
                        cardObject.transform.SetParent(space);
                    });
                });
            }
            cardObject.GetComponent<CardDisplay>().ChangeInteractions();
        }
        instance.ChangeBuyingPileTextValue();
        instance.ChangeDiscardPileTextValue();
    }
    public void ChangeBuyingPileTextValue()
    {
        int pileCount = GameplayManager.currentCombat.combatents[0].decks[0].BuyingPile.Count;
        if (pileCount == 0)
        {
            buyingPileText.text = "";
        }
        else
        {
            //GameObject g = GameplayManager.currentCombat.combatents[0].decks[0].BuyingPile.GetTop().cardDisplay.gameObject;
            buyingPileText.text = pileCount.ToString();
            float cardHeight = 0.03f;
            float yOffset = Mathf.Abs(((pileCount - 1) * cardHeight) + 0.01f);
            ///float yOffset = g.transform.position.y + 0.01f;
            buyingPileText.transform.position = buyingPilePos.position + Vector3.forward * 1.75f + Vector3.up * yOffset;
            //buyingPileText.gameObject.SetActive(true);
        }
    }

    public void ChangeDiscardPileTextValue()
    {
        int pileCount = GameplayManager.currentCombat.combatents[0].decks[0].DiscardPile.Count;
        if (pileCount == 0)
        {
            discardPileText.text = "";
        }
        else
        {
            discardPileText.text = pileCount.ToString();
            //discardPileText.gameObject.SetActive(true);
        }
        float cardHeight = 0.1f;
        float yOffset = ((pileCount - 1) * cardHeight) + 0.01f;
        discardPileText.transform.position = discardPilePos.position + Vector3.forward * 1.75f + Vector3.up * yOffset;
    }

    public void ActivateCardTextValues()
    {
        buyingPileText.gameObject.SetActive(true);
        discardPileText.gameObject.SetActive(true);
    }

    public void DeactivateCardTextValues()
    {
        buyingPileText.gameObject.SetActive(false);
        discardPileText.gameObject.SetActive(false);
    }

    /*public void ChangePileTextValues(Transform space)
    {
        if (space == buyingPilePos)
        {
            //buyingPileText.text = pile.Count.ToString();
            buyingPileText.text = GameplayManager.currentCombat.combatents[0].decks[0].BuyingPile.Count.ToString();
            buyingPileText.transform.position = buyingPilePos.position + Vector3.forward * 1.75f + Vector3.up * 0.25f;
        }
        else if (space == discardPilePos)
        {
            //discardPileText.text = pile.Count.ToString();
            discardPileText.text = GameplayManager.currentCombat.combatents[0].decks[0].DiscardPile.Count.ToString();
            discardPileText.transform.position = discardPilePos.position + Vector3.up * 0.25f;
        }
    }*/
}
