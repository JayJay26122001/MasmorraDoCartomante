using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using static UnityEditor.PlayerSettings;
#endif

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public SpriteRenderer cardBack, image, rarity, background, type;
    public MeshRenderer cardBase;
    public Material dissolveShader, cardBaseShader, cardBaseMat;
    public TextMeshPro cardCost, cardName, cardDescription;
    public Card cardData;
    public CardsUI cardsUI;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool hasSetOriginalTransform = false;
    bool highlighted;
    public CardPack pack;
    bool inAnimation = false, disappearing;
    float animTimeStart;
    public float shaderAnimSpeed;

    void Start()
    {
        if (!hasSetOriginalTransform)
        {
            originalScale = transform.localScale;
            hasSetOriginalTransform = true;
        }
        if(cardData.deck != null && cardData.deck.Owner != GameplayManager.instance.player)
        {
            SetupShader();
        }
    }

    void SetupShader()
    {
        dissolveShader.SetTexture("_MainTex", cardBack.material.mainTexture);
        cardBack.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", image.material.mainTexture);
        image.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", rarity.material.mainTexture);
        rarity.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", background.material.mainTexture);
        background.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", type.material.mainTexture);
        type.material = dissolveShader;
        cardBaseShader.SetTexture("_MainTex", cardBase.material.mainTexture);
        cardBase.material = cardBaseShader;
        /*dissolveShader.SetTexture("_MainTex", cardCost.material.mainTexture);
        cardCost.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", cardName.material.mainTexture);
        cardName.material = dissolveShader;
        dissolveShader.SetTexture("_MainTex", cardDescription.material.mainTexture);
        cardDescription.material = dissolveShader;*/
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
        if (cardData.Rarity == Card.CardRarity.Legendary)
        {
            type.sprite = cardsUI.cardDiamondType[(int)cardData.Type];
        }
        else
        {
            type.sprite = cardsUI.cardType[(int)cardData.Type];
        }
        //type.sprite = cardsUI.cardType[(int)cardData.Type];
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
                CameraController.instance.HighlightCard(gameObject.GetComponentsInChildren<Transform>()[1].position);
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
                if (c.hand.Contains(cardData) && c.GetComponent<Player>() != null /*&& GameplayManager.currentCombat.TurnIndex == 0*/)
                {
                    LeanTween.scale(gameObject, originalScale * 1.25f, 0.1f).setEaseOutQuad();
                    highlighted = true;
                    AudioController.instance.RandomizeSfx(AudioController.instance.sfxSource, AudioController.instance.receiveCardSfx);
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
                CardUIController.OrganizeHandCardsWhenHighlighted(c);
                highlighted = false;
            }
        }
    }

    public void AnimateEnemyCard(bool disappear)
    {
        disappearing = disappear;
        animTimeStart = Time.time;
        inAnimation = true;
    }

    float t = 0;
    private void Update()
    {
        if (inAnimation)
        {
            if (disappearing)
            {
                t = Mathf.Clamp((Time.time - animTimeStart) * shaderAnimSpeed, 0, 1);
            }
            else
            {
                t = Mathf.Clamp(1 - ((Time.time - animTimeStart) * shaderAnimSpeed), 0, 1);
            }
            cardBack.material.SetFloat("_DissolveAmount", t);
            image.material.SetFloat("_DissolveAmount", t);
            rarity.material.SetFloat("_DissolveAmount", t);
            background.material.SetFloat("_DissolveAmount", t);
            type.material.SetFloat("_DissolveAmount", t);
            cardBase.material.SetFloat("_DisappearTime", t);
            if ((t >= 1 && disappearing) || (t <= 0 && !disappearing))
            {
                inAnimation = false;
            }
            if (t >= 0.8 && disappearing)
            {
                cardCost.gameObject.SetActive(false);
                cardName.gameObject.SetActive(false);
                cardDescription.gameObject.SetActive(false);
            }
            if (t <= 0.2 && !disappearing)
            {
                cardCost.gameObject.SetActive(true);
                cardName.gameObject.SetActive(true);
                cardDescription.gameObject.SetActive(true);
            }
        }
    }
}

