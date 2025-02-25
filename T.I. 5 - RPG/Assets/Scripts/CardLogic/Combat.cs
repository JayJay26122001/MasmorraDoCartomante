using System;
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
        creature1.Enemy = creature2;
        creature2.Enemy = creature1;
        Round = new Turn[2] { new Turn(creature1), new Turn(creature2) };
        ActiveTurn = Round[turnIndex];
        TurnIndex = 0;
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
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log($"Turn {TurnIndex+1} {ActiveTurn.phase}");
            Debug.Log($"Turn {TurnIndex+1} {ActiveTurn.currentPhase}");
            AdvanceCombat();
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
        phases = new TurnPhase[] { new TurnStart(), new ReactionTurn(), new TurnEnd() };
        phaseIndex = 0;
        currentPhase = phases[phaseIndex];
    }
    public void TurnStart()
    {
        //phase = 0;
        phaseIndex = 0;
        currentPhase = phases[phaseIndex];
        TurnOwner.resetShield();
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
        }
        else
        {
            TurnStart();
        }

    }
}
public abstract class TurnPhase
{

}
public class TurnStart :TurnPhase
{

}
public class ReactionTurn :TurnPhase
{

}
public class TurnEnd :TurnPhase
{
    
}
