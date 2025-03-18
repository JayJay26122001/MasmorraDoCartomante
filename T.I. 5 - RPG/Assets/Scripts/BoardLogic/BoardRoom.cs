using UnityEngine;
using System.Collections.Generic;


public class BoardRoom
{
    public BoardRoomSO type;
    public List<int> nextRoomsProbabilities = new List<int>();
    public int nextRoomsCount, branchLevel;
    public List<BoardRoom> nextRooms = new List<BoardRoom>();
    public GameObject roomObject;
    public bool wantsToMerge;
    public BoardRoom (BoardRoomSO type) //Boss room only
    {
        this.type = type;
    }
    public BoardRoom (BoardRoomSO type, List<int> nrProbabilities, int nextCount, int branchLevel, bool wantsToMerge) //Other rooms
    {
        this.type = type;
        this.nextRoomsCount = nextCount;
        this.nextRoomsProbabilities = nrProbabilities;
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
}
