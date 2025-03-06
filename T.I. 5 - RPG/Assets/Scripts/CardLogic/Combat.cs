using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public Creature creature1, creature2;
    public Turn[] Round;
    public Turn ActiveTurn;
    int turnIndex = 0;
    public int TurnIndex { get{ return turnIndex; } private set { turnIndex = value % Round.Length; } }
    public void StartCombat()
    {
        ConditionObserver.currentCombat = this;
        creature1.Enemy = creature2;
        creature2.Enemy = creature1;
        foreach (Deck d in creature1.decks)
        {
            d.StartShuffle();
        }
        foreach (Deck d in creature2.decks)
        {
            d.StartShuffle();
        }
        Round = new Turn[2] { new Turn(creature1), new Turn(creature2) };
        ActiveTurn = Round[turnIndex];
        TurnIndex = 0;
        ActiveTurn.TurnStart();
    }
    public void AdvanceCombat()
    {
        /*if ((int)ActiveTurn.phase < Enum.GetNames(typeof(Turn.TurnPhase)).Length - 1)
        {
            ActiveTurn.NextPhase();
        }*/
        if (ActiveTurn.phaseIndex < ActiveTurn.phases.Length - 1)
        {
            ActiveTurn.NextPhase();
        }
        else
        {
            ChangeTurn();
        }
        
    }
    public void ChangeTurn()
    {
        TurnIndex++;
        ActiveTurn = Round[turnIndex];
        ActiveTurn.TurnStart();
    }


    //TESTE RETIRAR DEPOIS
    void Awake()
    {
        StartCombat();
        Debug.Log($"Turn {TurnIndex + 1} {ActiveTurn.currentPhase}");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log($"Turn {TurnIndex+1} {ActiveTurn.phase}");
            AdvanceCombat();
            Debug.Log($"Turn {TurnIndex + 1} {ActiveTurn.currentPhase}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            creature1.PlayCard(creature1.decks[0].cards[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            creature1.PlayCard(creature1.decks[0].cards[1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            creature1.PlayCard(creature1.decks[0].cards[2]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            creature2.PlayCard(creature2.decks[0].cards[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            creature2.PlayCard(creature2.decks[0].cards[1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            creature2.PlayCard(creature2.decks[0].cards[2]);
        }
    }
}
public class Turn
{
    //public enum TurnPhase { TurnStart, ReactionTurn, TurnEnd }
    //public TurnPhase phase { get; private set; }
    public TurnPhase[] phases;
    public TurnPhase currentPhase;
    public int phaseIndex;

    public Creature TurnOwner { get; private set; }
    public Turn(Creature owner)
    {
        TurnOwner = owner;
        phases = new TurnPhase[] { new TurnStart(owner), new ReactionTurn(owner), new TurnEnd(owner) };
        phaseIndex = 0;
        currentPhase = phases[phaseIndex];
    }
    public void TurnStart()
    {
        //phase = 0;
        phaseIndex = 0;
        currentPhase = phases[phaseIndex];
        currentPhase.StartPhase();

        TurnOwner.resetShield();
        TurnOwner.resetEnergy();
    }
    public void NextPhase()
    {
        /*if ((int)phase < Enum.GetNames(typeof(TurnPhase)).Length - 1)
        {
            phase++;
        }*/
        if (phaseIndex < phases.Length - 1)
        {
            phaseIndex++;
            currentPhase = phases[phaseIndex];
            currentPhase.StartPhase();
        }
        else
        {
            TurnStart();
        }

    }
}
public abstract class TurnPhase
{
    protected Creature owner;
    public TurnPhase(Creature owner)
    {
        this.owner = owner;
    }

    public virtual void StartPhase()
    {
        PhaseEffect();
    }
    public abstract void PhaseEffect();
}
public class TurnStart : TurnPhase
{
    public TurnStart(Creature owner) : base(owner) { }
    public override void PhaseEffect()
    {
        owner.BuyCards(owner.CardBuyMax);
        owner.canPlayCards = true;
    }
}
public class ReactionTurn : TurnPhase
{
    public ReactionTurn(Creature owner) : base(owner) { }
    public override void PhaseEffect()
    {
        owner.canPlayCards = false;
        List<Card> temp = owner.Enemy.playedCards.ToList();
        foreach (Card c in temp)
        {
            c.CheckConditions();
        }
        temp = owner.Enemy.playedCards.ToList();
        foreach (Card c in temp)
        {
            c.ConditionalCardFailled();
        }
    }
}
public class TurnEnd : TurnPhase
{
    public TurnEnd(Creature owner) : base(owner) { }
    public override void PhaseEffect()
    {
        owner.TriggerPlayedCards();
    }
}
