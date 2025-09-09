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
    public enum CardPack { Normal, Zodiac, EnemyExclusive, MinorArcana, MajorArcana }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }
    public CardType Type;
    public CardPack Pack;
    public CardRarity Rarity;
    [NonSerialized] public bool Temporary = false;
    [TextArea] public string Description;
    //public List<Condition.condition> conditions = new List<Condition.condition>();
    //List<Condition> conds = new List<Condition>();
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
            GameplayManager.currentCombat.CombatUI();
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
    public void RevertProperties()
    {
        limited = ogProperties.Limited;
        instantaneous = ogProperties.Instantaneous;
        cost = ogProperties.Cost;
    }
}
