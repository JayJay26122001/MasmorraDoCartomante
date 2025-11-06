using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardRoom
{
    public BoardRoomSO type;
    public List<ControlledProbability> nextRoomsProbabilities = new List<ControlledProbability>();
    public int nextRoomsCount, branchLevel;
    public List<BoardRoom> nextRooms = new List<BoardRoom>();
    public GameObject roomObject;
    public bool wantsToMerge;
    public BoardRoom (BoardRoomSO type) //Boss room only
    {
        this.type = type;
    }
    public BoardRoom (BoardRoomSO type, List<ControlledProbability> nrProbabilities, int nextCount, int branchLevel, bool wantsToMerge) //Other rooms
    {
        this.type = type;
        this.nextRoomsCount = nextCount;
        this.nextRoomsProbabilities = nrProbabilities;
        this.branchLevel = branchLevel;
        this.wantsToMerge = wantsToMerge;
    }
    public BoardRoom (int nextCount, int branchLevel, bool wantsToMerge) //Other rooms
    {
        this.nextRoomsCount = nextCount;
        this.branchLevel = branchLevel;
        this.wantsToMerge = wantsToMerge;
    }

    public bool CheckNextRooms(BoardRoomSO room)
    {
        bool contains = false;
        foreach(BoardRoom r in nextRooms)
        {
            if(r.type == room)
            {
                contains = true;
                break;
            }
        }
        return contains;
    }

    public bool CheckRoomRecursive(BoardRoomSO room, int levelsLeft)
    {
        if(type == room)
        {
            return true;
        }
        else
        {
            if(levelsLeft == 0)
            {
                return false;
            }
            else
            {
                bool contains = false;
                foreach (BoardRoom r in nextRooms)
                {
                    if (r.CheckRoomRecursive(room, levelsLeft - 1))
                    {
                        contains = true;
                    }
                }
                return contains;
            }
        }
    }

    public void GetNextRooms(int levelsLeft, List<BoardRoom> rooms)
    {
        rooms.Add(this);
        if(levelsLeft > 0)
        {
            foreach (BoardRoom r in nextRooms)
            {
                r.GetNextRooms(levelsLeft - 1, rooms);
            }
        }
    }
}

[Serializable]
public class ControlledProbability
{
    public string type;
    public int probability;
    public float multiplier = 1, minMult, maxMult;
    public bool canBeZero = true;

    public ControlledProbability(string type, int probability, float multiplier, float minMult, float maxMult, bool canBeZero)
    {
        this.type = type;
        this.probability = probability;
        this.multiplier = multiplier;
        this.minMult = minMult;
        this.maxMult = maxMult;
        this.canBeZero = canBeZero;
    }

    public void ModifyProbability(int value)
    {
        if(canBeZero)
        {
            probability = (int)Mathf.Clamp(((float)value * multiplier), 0, float.MaxValue);
        }
        else
        {
            probability = (int)Mathf.Clamp(((float)value * multiplier), 2, float.MaxValue);
        }
    }

    public void ModifyMultiplier(float diff)
    {
        multiplier = Mathf.Clamp(multiplier + diff, minMult, maxMult);
    }
}
