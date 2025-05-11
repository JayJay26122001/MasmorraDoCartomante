using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public SpriteRenderer rarity, background, type;
    public TextMeshPro cardCost, cardName, cardDescription;
    public Card cardData;
    public CardsUI cardsUI;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    //public bool isReadyToMove = true;
    private bool hasSetOriginalTransform = false;
    bool highlighted;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (!GameplayManager.instance.InputActive) return;
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
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
            cardData.deck.Owner.PlayCard(cardData);
            GameplayManager.currentCombat.CombatUI();
        }
        else
        {
            CameraController.instance.HighlightCard(this.transform.position);
        }
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
        /*if (GameplayManager.instance.InputActive)
        {
            CardUIController.instance.SetHighlightedCard(null);
        }*/
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
                    //LeanTween.cancel(gameObject);
                    LeanTween.scale(gameObject, originalScale * 1.25f, 0.1f).setEaseOutQuad();
                    //LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y + 1f, originalPosition.z + -0.5f), 0.15f).setEaseOutSine();
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
                //LeanTween.cancel(gameObject);
                LeanTween.scale(gameObject, originalScale, 0.1f).setEaseOutQuad();
                //LeanTween.moveLocal(gameObject, new Vector3(gameObject.transform.localPosition.x, originalPosition.y, originalPosition.z), 0.15f).setEaseOutSine();
                highlighted = false;
                //CardUIController.OrganizeHandCards(c);
            }
        }
    }
}

