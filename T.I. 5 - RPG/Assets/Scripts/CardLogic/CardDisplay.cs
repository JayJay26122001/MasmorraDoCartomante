using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;


#if UNITY_EDITOR
using static UnityEditor.PlayerSettings;
#endif

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public SpriteRenderer cardBack, image, rarity, background, type;
    [SerializeField] ParticleSystem ActivatedEffectVFX;
    [SerializeField] ParticleSystem[] ActivationVFX;
    [System.NonSerialized] public ParticleSystem SelectedActivationVFX;
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
    public GameObject outline;
    Interactable interactable;
    public ControlUI playCard, zoomIn, zoomOut, removeCard, cloneCard, addCard;
    public struct Token
    {
        public int index;
        public string var;
    }

    void Start()
    {
        interactable = this.gameObject.GetComponent<Interactable>();
        if (!hasSetOriginalTransform)
        {
            originalScale = transform.localScale;
            hasSetOriginalTransform = true;
        }
        if (cardData.deck != null && cardData.deck.Owner != GameplayManager.instance.player)
        {
            SetupShader();
        }
        if (outline != null)
        {
            outline.SetActive(false);
            switch(cardData.Type)
            {
                case Card.CardType.Attack:
                    outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                    break;
                case Card.CardType.Defense:
                    outline.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0, 0.3f, 1, 1));
                    break;
                case Card.CardType.Mind:
                    outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                    break;
            }
            outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.1f);
            outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
            outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.01f);
            outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.2f);
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
        if (!GameplayManager.instance.removingCards && !GameplayManager.instance.duplicatingCards)
        {
            card.cardDisplay = this;
        }
        CardSetup();
    }

    public void CardSetup()
    {
        UpdateCardCost();
        cardName.text = cardData.Name;
        cardDescription.text = cardData.Description;
        image.sprite = cardData.CardImage;
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
        SelectedActivationVFX = ActivationVFX[(int)cardData.Type];
        //type.sprite = cardsUI.cardType[(int)cardData.Type];
        Color effectcolor = Color.white;
        switch (cardData.Type)
        {
            case Card.CardType.Attack:
                effectcolor = Color.red;
                break;
            case Card.CardType.Defense:
                effectcolor = Color.blue;
                break;
            case Card.CardType.Mind:
                effectcolor = Color.green;
                break;
        }
        var main = ActivatedEffectVFX.main;
        main.startColor = effectcolor;
    }

    public void ChangeCostColor()
    {
        cardCost.color = Color.red;
        Invoke("ReturnCostColor", 1);
    }
    public void ReturnCostColor()
    {
        cardCost.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (pack == null)
            {
                if (!GameplayManager.instance.removingCards)
                {
                    if(!GameplayManager.instance.duplicatingCards)
                    {
                        LeanTween.scale(gameObject, originalScale, 0.15f).setEaseOutQuad();
                        cardData.deck.Owner.PlayCard(cardData);
                        GameplayManager.currentCombat.CombatUI();
                    }
                    else
                    {
                        if(!GameplayManager.instance.stamp.stamping)
                        {
                            if(GameplayManager.instance.player.ChangeMoney(-GameplayManager.instance.stamp.price))
                            {
                                CameraController.instance.DeActivateZoomedCamera();
                                if(CameraController.instance.highlightCardCamera.Priority == 2)
                                {
                                    CameraController.instance.HighlightCard(Vector3.zero, CameraController.instance.zoomedCard);
                                }
                                CameraController.instance.ChangeCamera(0);
                                GameplayManager.instance.duplicatingCards = false;
                                GameplayManager.instance.player.decks[0].AddCard(cardData);
                                GameplayManager.instance.duplicatingCards = true;
                                GameplayManager.instance.stamp.StartStampCards(cardData, this.gameObject);
                            }
                        }
                    }
                }
                else
                {
                    if (CameraController.instance.highlightCardCamera.Priority == 2)
                    {
                        CameraController.instance.HighlightCard(Vector3.zero, CameraController.instance.zoomedCard);
                    }
                    cardData.deck.RemoveCard(this);
                    GameplayManager.instance.DestroyRemovingCards();
                }
            }
            else
            {
                if (CameraController.instance.highlightCardCamera.Priority == 2)
                {
                    CameraController.instance.HighlightCard(Vector3.zero, CameraController.instance.zoomedCard);
                }
                GameplayManager.instance.player.decks[0].AddCard(cardData);
                pack.DestroyBoughtCards(cardData);
                pack = null;
            }
        }
        else
        {
            if (pack != null || GameplayManager.instance.duplicatingCards || GameplayManager.instance.removingCards || cardData.deck.Owner.playedCards.Contains(cardData))
            {
                CameraController.instance.HighlightCard(gameObject.GetComponentsInChildren<Transform>()[1].position, this);
            }
        }
        ChangeInteractions();
    }

    public void ChangeInteractions()
    {
        if(interactable != null)
        {
            interactable.HideInteractions();
            interactable.interactions.Clear();
            if(cardData.deck != null && cardData.deck.Owner.hand.Contains(cardData))
            {
                interactable.interactions.Add(playCard);
            }
            else if(pack != null || GameplayManager.instance.duplicatingCards || GameplayManager.instance.removingCards || (cardData.deck != null && cardData.deck.Owner.playedCards.Contains(cardData)))
            {
                if(CameraController.instance.zoomedCard != this)
                {
                    interactable.interactions.Add(zoomIn);
                }
                else
                {
                    interactable.interactions.Add(zoomOut);
                }

                if(pack != null)
                {
                    interactable.interactions.Add(addCard);
                }
                else if(GameplayManager.instance.duplicatingCards)
                {
                    interactable.interactions.Add(cloneCard);
                }
                else if(GameplayManager.instance.removingCards)
                {
                    interactable.interactions.Add(removeCard);
                }
            }
        }
    }

    private void OnMouseEnter()
    {
        if (pack != null || GameplayManager.instance.duplicatingCards || GameplayManager.instance.removingCards)
        {
            ChangeInteractions();
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

            if (pack != null || GameplayManager.instance.duplicatingCards || GameplayManager.instance.removingCards || cardData.deck.Owner.playedCards.Contains(cardData))
            {
                outline.SetActive(true);
            }
        }
        else if (highlighted)
        {
            CardUIController.instance.SetHighlightedCard(null);
        }
        if (cardData.deck != null && (cardData.deck.Owner.hand.Contains(this.cardData) || cardData.deck.Owner.playedCards.Contains(this.cardData)))
        {
            GameManager.instance.uiController.ShowPopups(this);
        }
        if(!string.IsNullOrEmpty(cardData.extraDescription)) 
        {
            UpdateCardExtraDescription();
        }
        if(GameplayManager.instance.duplicatingCards && !GameplayManager.instance.stamp.stamping)
        {
            GameplayManager.instance.stamp.SetPrice(this.cardData);
        }
    }

    public void OnMouseExit()
    {
        if (highlighted)
        {
            CardUIController.instance.SetHighlightedCard(null);
        }
        outline.SetActive(false);
        GameManager.instance.uiController.HidePopups();
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

    public void SetActivatedEffectVFX(bool active) 
    {
        if (active)
        {
            var main = ActivatedEffectVFX.main;
            main.loop = true;
            main.prewarm = true;
            ActivatedEffectVFX.Play();
        }
        else 
        {
            ActivatedEffectVFX.Stop();
        }
    }
    public void PlayActivatedEffectOnce()
    {
        if (!ActivatedEffectVFX.isPlaying)
        {
            var main = ActivatedEffectVFX.main;
            main.loop = false;
            ActivatedEffectVFX.Play();
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
        //UpdateCardBoolDescription();
        //UpdateValues();
    }

    /*public void UpdateCardBoolDescription()
    {
        string desc = cardData.Description;
        if (cardData.instantaneous && cardData.limited)
        {
            desc += " INSTANTANEOUS & LIMITED";
        }
        else if (cardData.instantaneous && !cardData.limited)
        {
            desc += " INSTANTANEOUS";
        }
        else if (!cardData.instantaneous && cardData.limited)
        {
            desc += " LIMITED";
        }
        cardDescription.text = desc;
    }*/

    //DESCRI��O MODULAR
    public void UpdateCardCost()
    {
        cardCost.text = cardData.cost.ToString();
    }
    public void UpdateCard()
    {
        string desc = cardData.Description;
        List<Token> tokens = FindValuesInString(desc);
        foreach (Token token in tokens)
        {
            string value = GetEffectValue(token.index, token.var);
            string pattern = $"v[{token.index}]{{{token.var}}}";
            desc = desc.Replace(pattern, value);
        }
        cardDescription.richText = true;
        if (cardData.instantaneous && cardData.limited)
        {
            desc += " <color=#FFD700>INSTANTANEOUS & LIMITED</color>";
        }
        else if (cardData.instantaneous && !cardData.limited)
        {
            desc += " <color=#FFD700>INSTANTANEOUS</color>";
        }
        else if (!cardData.instantaneous && cardData.limited)
        {
            desc += " <color=#FFD700>LIMITED</color>";
        }
        cardDescription.text = desc;
        UpdateCardCost();
    }

    public void UpdateCardExtraDescription()
    {
        string desc = cardData.extraDescription;
        List<Token> tokens = FindValuesInString(desc);
        foreach (Token token in tokens)
        {
            string value = GetEffectValue(token.index, token.var);
            string pattern = $"v[{token.index}]{{{token.var}}}";
            desc = desc.Replace(pattern, value);
        }
        GameManager.instance.uiController.ingamePopupDescription.text = desc;
    }

    public List<Token> FindValuesInString(string input)
    {
        List<Token> tokens = new List<Token>();
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == 'v' && i + 1 < input.Length && input[i + 1] == '[')
            {
                i += 2;
                int start = i;
                for (; i < input.Length && input[i] != ']'; i++) { }
                int effectIndex = int.Parse(input.Substring(start, i - start));
                if (i < input.Length && input[i] == ']') i++;
                if (i < input.Length && input[i] == '{') i++;
                start = i;
                for (; i < input.Length && input[i] != '}'; i++) { }
                string variable = input.Substring(start, i - start);
                if (i < input.Length && input[i] == '}') i++;
                tokens.Add(new Token { index = effectIndex, var = variable });
            }
        }
        return tokens;
    }

    string GetEffectValue(int effectIndex, string variable)
    {
        if (effectIndex < cardData.Effects.Count && effectIndex >= 0)
        {
            Effect observedEffect = cardData.Effects[effectIndex];
            Creature statOwner;
            if (cardData.deck != null && cardData.deck.Owner != null)
            {
                statOwner = cardData.deck.Owner;
            }
            else
            {
                statOwner = GameplayManager.instance.player;
            }
            switch (variable)
            {
                case "Damage":
                    if (observedEffect is DealDamage damage)
                    {
                        VarValue v = GetExpression(damage.DamageMultiplier);
                        int baseMultiplier = 1;
                        if (damage.MultipliedByBaseDamage)
                        {
                            baseMultiplier = statOwner.BaseDamage;
                        }
                        if (!v.isRandom)
                        {
                            int i = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.def, statOwner.DamageModifiers));
                            return $"<color=#FF5555>{i}</color>";
                        }
                        else
                        {
                            int min = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.min, statOwner.DamageModifiers));
                            int max = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.max, statOwner.DamageModifiers));
                            return $"<color=#FF5555>{min}-{max}</color>";
                        }
                        //return damage.GetDamage().ToString();
                        /*if (pack == null)
                        {
                            return $"<color=#FF5555>{damage.GetDamage()}</color>";
                        }
                        else
                        {
                            int dmg = (int)Mathf.Round(GameplayManager.instance.player.BaseDamage * damage.DamageMultiplier.GetValue());
                            return $"<color=#FF5555>{dmg}</color>";
                        }*/
                    }
                    break;
                case "Shield":
                    if (observedEffect is GainShield shield)
                    {
                        VarValue v = GetExpression(shield.ShieldMultiplier);
                        int baseMultiplier = 1;
                        if (shield.MultipliedByBaseShield)
                        {
                            baseMultiplier = statOwner.BaseShieldGain;
                        }
                        if (!v.isRandom)
                        {
                            int i = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.def, statOwner.ShieldModifiers));
                            return $"<color=#55AAFF>{i}</color>";
                        }
                        else
                        {
                            int min = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.min, statOwner.ShieldModifiers));
                            int max = (int)Math.Round(StatModifier.ApplyModfierList(baseMultiplier * v.max, statOwner.ShieldModifiers));
                            return $"<color=#55AAFF>{min}-{max}</color>";
                        }
                        //return shield.GetShield().ToString();
                        /*if (pack == null)
                        {
                            return $"<color=#55AAFF>{shield.GetShield()}</color>";
                        }
                        else
                        {
                            int shld = (int)Mathf.Round(GameplayManager.instance.player.BaseShieldGain * shield.ShieldMultiplier.GetValue());
                            return $"<color=#55AAFF>{shld}</color>";
                        }*/
                    }
                    break;
            }
        }
        return "Invalid Value";
    }
    struct VarValue
    {
        public bool isRandom;
        public float def, min, max;

    }
    VarValue GetExpression(ModularVar V)
    {
        VarValue value = new VarValue();
        value.isRandom = V.HasRandom();
        if (!value.isRandom)
        {
            if (V is RecursiveInt i)
            {
                value.def = i.GetValue();
            }
            else if (V is RecursiveFloat f)
            {
                value.def = f.GetValue();
            }
            /*else if (V is ModularInt mi)
            {
                value.def = mi.GetValue();
            }
            else if (V is ModularFloat mf)
            {
                value.def = mf.GetValue();
            }*/
        }
        else
        {
            if (V is RecursiveInt i)
            {
                value.min = i.GetMinValue();
                value.max = i.GetMaxValue();
            }
            else if (V is RecursiveFloat f)
            {
                value.min = f.GetMinValue();
                value.max = f.GetMaxValue();
            }
            /*else if (V is ModularInt mi)
            {
                value.min = mi.GetMinValue();
                value.max = mi.GetMaxValue();
            }
            else if (V is ModularFloat mf)
            {
                value.min = mf.GetMinValue();
                value.max = mf.GetMaxValue();
            }*/
        }
        return value;
    }
    public int VerifyCardPopupQuantity()
    {
        int popupQuantity = 0;
        if (cardData.instantaneous)
        {
            popupQuantity++;
        }
        if (cardData.limited)
        {
            popupQuantity++;
        }
        if (!string.IsNullOrEmpty(cardData.extraDescription))
        {
            popupQuantity++;
        }
        return popupQuantity;
    }
}

