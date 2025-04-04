using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Combat : MonoBehaviour
{
    public CardCombatSpaces[] combatSpaces;
    public Creature[] combatents;
    public Turn[] Round;
    public Turn ActiveTurn;
    int turnIndex = 0;
    public TextMeshProUGUI turnText;
    public int TurnIndex { get{ return turnIndex; } private set { turnIndex = value % Round.Length; } }
    public void StartCombat()
    {
        GameplayManager.currentCombat = this;
        Round = new Turn[2] { new Turn(combatents[0]), new Turn(combatents[1]) };
        for (int i = 0; i < 2; i++)
        {
            combatents[i].Enemy = combatents[(i + 1) % 2];
            foreach (Deck d in combatents[i].decks)
            {
                d.StartShuffle();
            }
            combatents[i].combatSpace = combatSpaces[i];
            combatents[i].CombatStartAction();

        }
        ActiveTurn = Round[turnIndex];
        TurnIndex = 0;
        ActiveTurn.TurnStart();
        CardUIController.CardsOrganizer(combatents[0]);
        CardUIController.CardsOrganizer(combatents[1]);
        CombatUI();
        turnText.text = $"Creature {TurnIndex + 1} - Turn {TurnIndex + 1} {ActiveTurn.currentPhase}";
        //combatents[0].CardsOrganizer(); //mudan�a futura
        //combatents[1].CardsOrganizer(); //mudan�a futura
    }
    public void AdvanceCombat()
    {
        /*if ((int)ActiveTurn.phase < Enum.GetNames(typeof(Turn.TurnPhase)).Length - 1)
        {
            ActiveTurn.NextPhase();
        }*/
        ActiveTurn.currentPhase.EndPhase();
        if (ActiveTurn.phaseIndex < ActiveTurn.phases.Length - 1)
        {
            ActiveTurn.NextPhase();
        }
        else
        {
            ChangeTurn();
        }
        CardUIController.CardsOrganizer(combatents[0]);
        CardUIController.CardsOrganizer(combatents[1]);
        CombatUI();
        //combatents[0].CardsOrganizer(); //mudan�a futura
        //combatents[1].CardsOrganizer(); //mudan�a futura
    }
    public void ChangeTurn()
    {
        TurnIndex++;
        ActiveTurn = Round[turnIndex];
        ActiveTurn.TurnStart();
    }

    public void CombatUI()
    {
        combatents[0].UpdateCreatureUI(combatents[0]);
        combatents[1].UpdateCreatureUI(combatents[1]);
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
            turnText.text = $"Creature {TurnIndex + 1} - Turn {TurnIndex + 1} {ActiveTurn.currentPhase}";
        }
        /*if (Input.GetKeyDown(KeyCode.Alpha1))
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
        }*/
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
        TurnOwner.resetShield();
        TurnOwner.resetEnergy();
        currentPhase.StartPhase();
    }
    public void NextPhase()
    {
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
    public virtual void EndPhase()
    {
        
    }
}
public class TurnStart : TurnPhase
{
    public TurnStart(Creature owner) : base(owner) { }
    public override void PhaseEffect()
    {
        //owner.BuyCards(owner.CardBuyMax);
        owner.canPlayCards = true;
        owner.TurnAction();
    }
    public override void EndPhase()
    {
        base.EndPhase();
        owner.GetComponent<Player>()?.DiselectCard();
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
