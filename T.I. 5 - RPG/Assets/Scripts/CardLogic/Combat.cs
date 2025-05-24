using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.Events;

public class Combat : MonoBehaviour
{
    public enum TurnPhaseTypes {Start, Reaction, End}
    public CardCombatSpaces[] combatSpaces;
    public Creature[] combatents;
    public Turn[] Round;
    public Turn ActiveTurn;
    int turnIndex = 0;
    public TextMeshProUGUI turnText;
    public int TurnIndex { get { return turnIndex; } private set { turnIndex = value % Round.Length; } }

    public void SetEnemy(int index)
    {
        combatents[1] = GameplayManager.instance.enemies[index];
    }
    public TurnPhase GetTurnPhase(Creature C, TurnPhaseTypes phaseType)
    {
        Turn turn = null;
        foreach (Turn t in Round)
        {
            if (t.TurnOwner == C)
            {
                turn = t;
            }
        }
        switch (phaseType)
        {
            case TurnPhaseTypes.Start:
                return turn.phases[0];
            case TurnPhaseTypes.Reaction:
                return turn.phases[1];
            case TurnPhaseTypes.End:
                return turn.phases[2];
            default:
                return null;
        }
    }

    public void StartCombat()
    {
        GameplayManager.currentCombat = this;
        GameplayManager.instance.SelectEnemy();
        Round = new Turn[2] { new Turn(combatents[0]), new Turn(combatents[1]) };
        for (int i = 0; i < 2; i++)
        {
            combatents[i].enemy = combatents[(i + 1) % 2];
            foreach (Deck d in combatents[i].decks)
            {
                d.StartShuffle();
            }
            combatents[i].combatSpace = combatSpaces[i];
            combatents[i].CombatStartAction();

        }
        TurnIndex = 0;
        ActiveTurn = Round[turnIndex];
        SceneAnimationController.instance.InvokeTimer(ActiveTurn.TurnStart, 1);
        SceneAnimationController.instance.InvokeTimer(CombatUI, 1);
        CardUIController.CardsOrganizer(combatents[0]);
        CardUIController.CardsOrganizer(combatents[1]);
        //CombatUI();
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
        GameplayManager.instance.PauseInput(0.5f);
        ActiveTurn.currentPhase.EndPhase();
        if (ActiveTurn.phaseIndex < ActiveTurn.phases.Length - 1)
        {
            ActiveTurn.NextPhase();
        }
        else
        {
            ChangeTurn();
        }
        //CardUIController.CardsOrganizer(combatents[0]);
        //CardUIController.CardsOrganizer(combatents[1]);
        turnText.text = $"Creature {TurnIndex + 1} - Turn {TurnIndex + 1} {ActiveTurn.currentPhase}";
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
    public void EndCombat()
    {
        foreach (Creature c in combatents)
        {
            c.EndCombat();
        }
        foreach (Turn t in Round)
        {
            foreach (TurnPhase p in t.phases)
            {
                p.PhaseStarted.RemoveAllListeners();
                p.PhaseEnded.RemoveAllListeners();
            }
        }
        GameplayManager.instance.ChangeBattleCount();
        GameplayManager.instance.HideAllEnemies();
    }


    //TESTE RETIRAR DEPOIS
    void Awake()
    {
        //SetEnemy();
        //StartCombat();
        //Debug.Log($"Turn {TurnIndex + 1} {ActiveTurn.currentPhase}");
    }
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceCombat();
            turnText.text = $"Creature {TurnIndex + 1} - Turn {TurnIndex + 1} {ActiveTurn.currentPhase}";
        }
    }*/
    public static void WaitForTurn(int TurnFromNow, TurnPhase phase, TurnPhase.PhaseTime phaseTime , UnityAction action)
    {
        UnityAction tymedAction = action;
        int turnsLeft = TurnFromNow;
        UnityAction onPhaseStarted = null;
        UnityEvent selectedEvent;
        switch (phaseTime)
        {
            case TurnPhase.PhaseTime.Start:
                selectedEvent = phase.PhaseStarted;
                break;
            case TurnPhase.PhaseTime.End:
                selectedEvent = phase.PhaseEnded;
                break;

            default:
                selectedEvent = phase.PhaseStarted;
                break;
        }
        onPhaseStarted = () =>
        {
            if (turnsLeft <= 0)
            {
                action.Invoke();
                selectedEvent.RemoveListener(onPhaseStarted); // Remove o listener após execução
            }
            else
            {
                turnsLeft--;
            }
        };
        selectedEvent.AddListener(onPhaseStarted);
    }
    public static void WaitForTurn<T>(int TurnFromNow, TurnPhase phase, TurnPhase.PhaseTime phaseTime, UnityAction<T> action, T arg)
    {
        UnityAction tymedAction = () => action(arg);
        int turnsLeft = TurnFromNow;
        UnityAction onPhaseStarted = null;
        UnityEvent selectedEvent;
        switch (phaseTime)
        {
            case TurnPhase.PhaseTime.Start:
                selectedEvent = phase.PhaseStarted;
                break;
            case TurnPhase.PhaseTime.End:
                selectedEvent = phase.PhaseEnded;
                break;

            default:
                selectedEvent = phase.PhaseStarted;
                break;
        }
        onPhaseStarted = () =>
        {
            if (turnsLeft <= 0)
            {
                tymedAction.Invoke();
                selectedEvent.RemoveListener(onPhaseStarted); // Remove o listener após execução
            }
            else
            {
                turnsLeft--;
            }
        };
        selectedEvent.AddListener(onPhaseStarted);
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
        TurnOwner.ResetShield();
        TurnOwner.ResetEnergy();
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
    public enum PhaseTime {Start, End}
    protected Creature owner;
    public TurnPhase(Creature owner)
    {
        this.owner = owner;
    }
    public UnityEvent PhaseStarted = new UnityEvent();
    public UnityEvent PhaseEnded = new UnityEvent();

    public virtual void StartPhase()
    {
        PhaseStarted.Invoke();
        PhaseEffect();
    }
    public abstract void PhaseEffect();
    public virtual void EndPhase()
    {
        PhaseEnded.Invoke();
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
        List<Card> temp = owner.enemy.playedCards.ToList();
        foreach (Card c in temp)
        {
            c.CheckConditions();
        }
        temp = owner.enemy.playedCards.ToList();
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
