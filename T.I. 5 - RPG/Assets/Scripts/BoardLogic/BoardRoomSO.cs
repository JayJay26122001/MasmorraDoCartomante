using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardRoomSO", menuName = "Scriptable Objects/BoardRoomSO")]
public class BoardRoomSO : ScriptableObject
{
    public string roomName;
    //public List<BoardRoomSO> possibleNextRooms = new List<BoardRoomSO>();
    public List<ControlledProbability> baseProbabilities = new List<ControlledProbability>();
    public Color32 testColor;
    public Mesh iconMesh;
}
