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
        if ((int)ActiveTurn.phase < Enum.GetNames(typeof(Turn.TurnPhase)).Length - 1)
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
            Debug.Log($"Turn {TurnIndex+1} {ActiveTurn.phase}");
            AdvanceCombat();
        }
    }
}
public class Turn
{
    public enum TurnPhase { TurnStart, ReactionTurn, TurnEnd }
    public TurnPhase phase { get; private set; }

    public Creature TurnOwner { get; private set; }
    public Turn(Creature owner)
    {
        TurnOwner = owner;
    }
    public void TurnStart()
    {
        phase = 0;
        TurnOwner.resetShield();
    }
    public void NextPhase()
    {
        if ((int)phase < Enum.GetNames(typeof(TurnPhase)).Length - 1)
        {
            phase++;
        }
        else
        {
            TurnStart();
        }

    }
}
