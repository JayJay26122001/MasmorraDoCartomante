using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CardDisplay : MonoBehaviour
{
    public SpriteRenderer rarity, background, type;
    public TextMeshPro cardCost, cardName, cardDescription;
    public Card cardData;
    public CardsUI cardsUI;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    public bool isReadyToMove = false;
    private bool hasSetOriginalTransform = false;

    void Start()
    {
        if (!hasSetOriginalTransform)
        {
            originalScale = transform.localScale;
            //originalPosition = transform.localPosition;
            hasSetOriginalTransform = true;
        }
    }

    public void SetCard(Card card)
    {
        cardData = card;
        card.cardDisplay = this;
        CardSetup();
    }

    public void CardSetup()
    {
        cardCost.text = cardData.cost.ToString();
        cardName.text = cardData.Name;
        cardDescription.text = cardData.Description;
        rarity.sprite = cardsUI.cardRarity[(int)cardData.Rarity];
        background.sprite = cardsUI.cardRarityBackground[(int)cardData.Rarity];
        type.sprite = cardsUI.cardType[(int)cardData.Type];
    }

    public void OnCardClick()
    {
        //cardData.deck.Owner.PlayCard(cardData); //substituir pela linha de baixo mais tarde
        /*if (cardData.deck.Owner.GetComponent<Player>()?.SelectedCard == cardData)
        {
            //cardData.deck.Owner.GetComponent<Player>().DiselectCard(cardData); PRA DESCELECIONAR A CARTA SE CLICAR DNV NELA
            cardData.deck.Owner.GetComponent<Player>().PlaySelectedCard();
        }
        else
        {
            cardData.deck.Owner.GetComponent<Player>()?.SelectCard(cardData);
        }*/
        LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
        cardData.deck.Owner.PlayCard(cardData);
        GameplayManager.currentCombat.CombatUI();
    }

    public void UpdatePosition()
    {
        originalPosition = transform.localPosition;
    }

    /*public void OnPointerEnterCard()
    {
        if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
        {
            Creature c = cardData.deck.Owner;
            if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null && GameplayManager.currentCombat.TurnIndex == 0)
            {
                if (isReadyToMove)
                {
                    LeanTween.scale(gameObject, originalScale * 1.25f, 0.15f).setEaseOutQuad();
                    LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y + 1f, originalPosition.z + -0.5f), 0.15f).setEaseOutSine();
                }
            }
        }
    }*/
    
    /*public void OnPointerExitCard()
    {
        if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
        {
            Creature c = cardData.deck.Owner;
            if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null)
            {
                LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
                LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y, originalPosition.z), 0.15f).setEaseOutSine();
            }
        }
    }*/

    public void OnMouseOver()
    {
        if (isReadyToMove)
        {
            if (gameObject.transform.localScale == originalScale)
            {
                if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
                {
                    Creature c = cardData.deck.Owner;
                    if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null && GameplayManager.currentCombat.TurnIndex == 0)
                    {
                        LeanTween.scale(gameObject, originalScale * 1.25f, 0.15f).setEaseOutQuad();
                        LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y + 1f, originalPosition.z + -0.5f), 0.15f).setEaseOutSine();
                    }
                }
            }
        }
    }

    public void OnMouseExit()
    {
        if (isReadyToMove)
        {
            if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
            {
                Creature c = cardData.deck.Owner;
                if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null)
                {
                    LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
                    LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y, originalPosition.z), 0.15f).setEaseOutSine();
                }
            }
        }
    }
}

