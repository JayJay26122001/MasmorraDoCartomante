using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using static UnityEditor.PlayerSettings;

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public SpriteRenderer rarity, background, type;
    public TextMeshPro cardCost, cardName, cardDescription;
    public Card cardData;
    public CardsUI cardsUI;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool hasSetOriginalTransform = false;
    bool highlighted;
    public CardPack pack;
    void Start()
    {
        if (!hasSetOriginalTransform)
        {
            originalScale = transform.localScale;
            hasSetOriginalTransform = true;
        }
    }

    public void SetCard(Card card)
    {
        cardData = card;
        if(!GameplayManager.instance.removingCards)
        {
            card.cardDisplay = this;
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(pack == null)
            {
                if(!GameplayManager.instance.removingCards)
                {
                    LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
                    cardData.deck.Owner.PlayCard(cardData);
                    GameplayManager.currentCombat.CombatUI();
                }
                else
                {
                    cardData.deck.RemoveCard(this);
                    GameplayManager.instance.DestroyRemovingCards();
                }
            }
            else
            {
                GameplayManager.instance.player.decks[0].AddCard(cardData);
                pack.DestroyBoughtCards();
                pack = null;
            }
        }
        else
        {
            if (pack != null || GameplayManager.instance.removingCards || cardData.deck.Owner.playedCards.Contains(cardData))
            {
                CameraController.instance.HighlightCard(this.transform.position);
            }
        }
    }

    public void UpdatePosition()
    {
        originalPosition = transform.localPosition;
    }
    public void OnMouseOver()
    {
        if (GameplayManager.instance.InputActive)
        {
            CardUIController.instance.SetHighlightedCard(cardData);
        }
        else if (highlighted)
        {
            CardUIController.instance.SetHighlightedCard(null);
        }
    }

    public void OnMouseExit()
    {
        if (highlighted)
        {
            CardUIController.instance.SetHighlightedCard(null);
        }
    }
    
    public void HighlightCard()
    {
        if (gameObject.transform.localScale == originalScale)
        {
            if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
            {
                Creature c = cardData.deck.Owner;
                if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null && GameplayManager.currentCombat.TurnIndex == 0)
                {
                    LeanTween.scale(gameObject, originalScale * 1.25f, 0.1f).setEaseOutQuad();
                    highlighted = true;
                    CardUIController.OrganizeHandCardsWhenHighlighted(c);
                }
            }
        }
    }

    public void UnhighlightCard()
    {
        if (cardData != null && cardData.deck != null && cardData.deck.Owner != null)
        {
            Creature c = cardData.deck.Owner;
            if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null)
            {
                LeanTween.scale(gameObject, originalScale, 0.1f).setEaseOutQuad();
                /*if (CardUIController.instance.highlightedCard == cardData)
                {
                    LeanTween.moveLocal(gameObject, originalPosition, 0.03f).setEaseInOutSine();
                }*/
                CardUIController.OrganizeHandCardsWhenUnhighlighted(c);
                highlighted = false;
            }
        }
    }
}

