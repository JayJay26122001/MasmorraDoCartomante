using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardSO", menuName = "Scriptable Objects/BoardSO")]
public class BoardSO : ScriptableObject
{
    public int levelsCount, maxBranches;
    public BoardRoomSO startRoom, bossRoom, battleRoom, shopRoom;
    public List<BoardRoomSO> roomList = new List<BoardRoomSO>();
    public SimpleInt moneyPrize;
    public int doubleBattleChance;
    public bool tripleBattle;
    public int tripleBattleChance;
}
