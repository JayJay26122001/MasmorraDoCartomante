using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Card", menuName = "CardLogic/Card")]
public class Card : ScriptableObject
{
    [SerializeReference]
    public List<Effect> Effects = new List<Effect>();
    public CardDisplay cardDisplay;
    public Deck deck;
    public string Name;
    public bool /*hidden,*/ limited, instantaneous;
    public int cost;
    public enum CardType { Attack, Defense, Mind }
    public enum CardPack { Normal, Zodiac, EnemyExclusive, MinorArcana, MajorArcana, SandsOfTime, PowerSurge }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }

    public CardType Type;
    public CardPack Pack;
    public CardRarity Rarity;
    [NonSerialized] public bool Temporary = false;
    [TextArea] public string Description, extraDescription;
    public Sprite CardImage;
    [SerializeField] private int id = -1;
    public int ID { get { return id; } }
    public void ChangeID(int value)
    {
        if (value < -1) return;
        id = value;
    }
    [ContextMenu("Reset ID")]
    public void ResetID()
    {
        id = -1;
        EditorUtility.SetDirty(this);
    }
    public void Setup() // chamado quando uma carta é adicionada ao deck
    {
        /*foreach (Condition.condition c in conditions)
        {
            conds.Add(new Condition(c, this));
        }*/
        if (instantaneous && !Effects.OfType<IProlongedEffect>().Any()) // seta quando cartas instantaneas serão descartadas
        {
            DiscardThisCard discard = new DiscardThisCard();
            discard.SetAsDiscardTime = DiscardThisCard.DiscardType.Minimum;
            WaitUntilTurn time = new WaitUntilTurn();
            time.TurnsFromNow = new ModularInt();
            time.TurnsFromNow.value = 0;
            time.TurnPhase = Combat.TurnPhaseTypes.End;
            time.PhaseTime = TurnPhase.PhaseTime.Start;
            discard.Conditions.Add(time);
            Effects.Add(discard);
        }
        ogProperties = new ModifiableProperties();
        ogProperties.Limited = limited;
        ogProperties.Instantaneous = instantaneous;
        ogProperties.Cost = cost;
        /*foreach (Effect effect in Effects) 
        {
            if(effect is IProlongedEffect e)
            {
                e.EffectApplied.AddListener(() => cardDisplay.SetActivatedEffectVFX(true));
                effect.EffectEnd.AddListener(() => cardDisplay.SetActivatedEffectVFX(false));
            }
        }*/
    }
    public void CardPlayed() // carta foi jogada na mesa
    {
        //AudioController.instance.RandomizeSfx(AudioController.instance.sfxSource, AudioController.instance.receiveCardSfx); //removido porque o inimigo também fazia esse barulho
        foreach (Effect e in Effects)
        {
            e.InitiateEffect();
        }
        if (instantaneous)
        {
            ApplyUnconditionalEffects();
            CardUIController.CardsOrganizer(deck.Owner);
            //GameplayManager.currentCombat.CombatUI();
        }
    }
    public void ApplyUnconditionalEffects() //aplica todos os efeitos que não tem condições
    {
        foreach (Effect e in Effects)
        {
            e.ApplyIfNoCondition();
        }
    }
    [NonSerialized] ModifiableProperties ogProperties;
    struct ModifiableProperties
    {
        public bool Limited, Instantaneous;
        public int Cost;
    }
    public void RevertAllProperties()
    {
        limited = ogProperties.Limited;
        instantaneous = ogProperties.Instantaneous;
        cost = ogProperties.Cost;
        cardDisplay.UpdateCard();
        cardDisplay.UpdateCardCost();
    }
    public void RevertLimited()
    {
        limited = ogProperties.Limited;
        cardDisplay.UpdateCard();
    }
    public void RevertInstantaneous()
    {
        instantaneous = ogProperties.Instantaneous;
        cardDisplay.UpdateCard();
    }
    public void RevertCost()
    {
        cost = ogProperties.Cost;
        cardDisplay.UpdateCardCost();
    }
}
