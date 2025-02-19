using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardRoomSO", menuName = "Scriptable Objects/BoardRoomSO")]
public class BoardRoomSO : ScriptableObject
{
    public string roomName;
    public List<BoardRoomSO> possibleNextRooms = new List<BoardRoomSO>();
    public List<int> nextRoomsProbabilities = new List<int>();
    public int nextRoomsCount;
    public List<BoardRoomSO> nextRooms = new List<BoardRoomSO>();
}
