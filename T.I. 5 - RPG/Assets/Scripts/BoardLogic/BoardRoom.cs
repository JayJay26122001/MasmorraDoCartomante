using UnityEngine;
using System.Collections.Generic;


public class BoardRoom
{
    public BoardRoomSO type;
    public List<int> nextRoomsProbabilities = new List<int>();
    public int nextRoomsCount;
    public List<BoardRoom> nextRooms = new List<BoardRoom>();
    public GameObject roomObject;
    public BoardRoom (BoardRoomSO type) //Boss room only
    {
        this.type = type;
    }
    public BoardRoom (BoardRoomSO type, List<int> probabilities, int nextCount) //Other rooms
    {
        this.type = type;
        this.nextRoomsCount = nextCount;
        this.nextRoomsProbabilities = probabilities;
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
