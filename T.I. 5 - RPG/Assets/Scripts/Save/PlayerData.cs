using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class PlayerData
{
    public List<string> playerDeckCards;
    public Vector3 boardPos;
    public Vector3 piecePos;
    public int currentRoomLevel;
    public int currentRoomIndex;
    public int money;
    public int hp;
}
